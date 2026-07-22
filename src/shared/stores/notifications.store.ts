import { create } from 'zustand';
import { persist } from 'zustand/middleware';

/**
 * Unread notification count for the topbar bell badge — a synced cache fed by
 * SignalR events / query invalidation (see docs/realtime/signalr-strategy.md),
 * not an independent source of truth (the full notification list lives in
 * TanStack Query). See docs/state/zustand-strategy.md.
 *
 * `readIds` exists because `UserNotificationSummaryResponse` (the list shape)
 * has no `readAt` field and `NotificationStatus` is an opaque, unpublished
 * enum (see docs/backend/notification/README.md) — there is no way to
 * determine "unread" from the list response without guessing which numeric
 * status value means what. Instead, this tracks ids this browser has
 * explicitly marked read through this UI; "unread" is computed as "loaded and
 * not in this set." Same browser-scoped caveat as Orders/Warehouses'
 * local-tracking stores — a notification marked read from another
 * client/session won't be reflected here. See
 * docs/modules/notification-center.md.
 */
interface NotificationsState {
  unreadCount: number;
  setUnreadCount: (count: number) => void;
  increment: () => void;
  readIds: string[];
  markRead: (id: string) => void;
  isRead: (id: string) => boolean;
}

export const useNotificationsStore = create<NotificationsState>()(
  persist(
    (set, get) => ({
      unreadCount: 0,
      setUnreadCount: (count) => set({ unreadCount: count }),
      increment: () => set((s) => ({ unreadCount: s.unreadCount + 1 })),
      readIds: [],
      markRead: (id) =>
        set((s) => ({ readIds: s.readIds.includes(id) ? s.readIds : [...s.readIds, id] })),
      isRead: (id) => get().readIds.includes(id),
    }),
    { name: 'simpleshopui-notifications', partialize: (s) => ({ readIds: s.readIds }) },
  ),
);
