'use client';

import { useCallback } from 'react';
import { useQueryClient, type InfiniteData } from '@tanstack/react-query';
import { useHubEvent } from '@/shared/lib/realtime/useHubEvent';
import { useNotificationsStore } from '@/shared/stores/notifications.store';
import { notificationsListQueryKey } from '../api/notifications.queries';
import type { CursorPaginatedResult } from '@/services/shared/paginated-result';
import type { UserNotificationSummaryResponse } from '@/services/notification';

export type NotificationCreatedEvent = UserNotificationSummaryResponse;

/**
 * TODO: `'NotificationCreated'` is a placeholder event name — no SignalR
 * event catalog is published by any of the 7 backend services (see
 * docs/realtime/signalr-strategy.md "Event catalog"). On receipt: prepends
 * the new item into the first cached page directly via `setQueryData` (so an
 * already-open dropdown shows it immediately with no refetch) and increments
 * the unread counter — never a full list invalidation, per "invalidate only
 * affected queries, avoid full-page refreshes."
 */
export function useNotificationRealtimeUpdates() {
  const queryClient = useQueryClient();
  const increment = useNotificationsStore((s) => s.increment);

  const handler = useCallback(
    (event: NotificationCreatedEvent) => {
      queryClient.setQueryData<
        InfiniteData<CursorPaginatedResult<UserNotificationSummaryResponse>>
      >(notificationsListQueryKey(), (old) => {
        if (!old) return old;
        const [firstPage, ...rest] = old.pages;
        if (!firstPage) return old;
        return {
          ...old,
          pages: [{ ...firstPage, items: [event, ...firstPage.items] }, ...rest],
        };
      });
      increment();
    },
    [queryClient, increment],
  );

  useHubEvent<NotificationCreatedEvent>('NotificationCreated', handler);
}
