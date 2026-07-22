'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  listMyUserNotifications,
  getUserNotification,
  markUserNotificationAsRead,
} from '@/services/notification';
import { useCursorList } from '@/shared/hooks/useCursorList';
import { useNotificationsStore } from '@/shared/stores/notifications.store';

const PAGE_SIZE = 15;

export const notificationKeys = {
  all: ['notifications'] as const,
  lists: () => [...notificationKeys.all, 'list'] as const,
  details: () => [...notificationKeys.all, 'detail'] as const,
  detail: (id: string) => [...notificationKeys.details(), id] as const,
};

/** Exact query key `useNotificationsListQuery` resolves to — exported so the realtime hook can target the same cache entry without duplicating the params/pageSize literals. */
export function notificationsListQueryKey() {
  return [...notificationKeys.lists(), {}, PAGE_SIZE] as const;
}

/**
 * Single cursor-list query shared by NotificationBell (fetches just page 1 on
 * mount — bounded, "lightweight," not the whole history) and
 * NotificationDropdown (loads more as the user scrolls). Same query key means
 * React Query dedupes the initial fetch rather than both components
 * re-requesting page 1 independently.
 */
export function useNotificationsListQuery() {
  return useCursorList({
    queryKey: notificationKeys.lists(),
    queryFn: listMyUserNotifications,
    params: {},
    limit: PAGE_SIZE,
  });
}

export function useNotificationQuery(notificationId: string) {
  return useQuery({
    queryKey: notificationKeys.detail(notificationId),
    queryFn: () => getUserNotification(notificationId),
    enabled: Boolean(notificationId),
  });
}

/**
 * Marks read via the real per-item endpoint, records it in
 * `notifications.store.ts`'s `readIds` (the only "read" signal this frontend
 * has, see docs/modules/notification-center.md), and decrements the unread
 * counter — never invalidates the whole list, only this one notification's
 * detail query, since the list itself doesn't carry a read/unread field to
 * refetch differently anyway.
 */
export function useMarkAsReadMutation() {
  const queryClient = useQueryClient();
  const markRead = useNotificationsStore((s) => s.markRead);
  const isRead = useNotificationsStore((s) => s.isRead);
  const decrement = () =>
    useNotificationsStore.setState((s) => ({ unreadCount: Math.max(0, s.unreadCount - 1) }));

  return useMutation({
    mutationFn: (notificationId: string) => markUserNotificationAsRead(notificationId),
    onSuccess: (_data, notificationId) => {
      if (!isRead(notificationId)) {
        markRead(notificationId);
        decrement();
      }
      queryClient.invalidateQueries({ queryKey: notificationKeys.detail(notificationId) });
    },
  });
}
