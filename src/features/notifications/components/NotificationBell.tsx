'use client';

import { useMemo, useState } from 'react';
import { useTranslations } from 'next-intl';
import { Bell } from 'lucide-react';
import { AppPopover, AppPopoverTrigger, AppPopoverContent, IconButton } from '@/shared/ui';
import { UnreadBadge } from '@/shared/notifications';
import { useSignalrStore } from '@/shared/stores/signalr.store';
import { useNotificationsStore } from '@/shared/stores/notifications.store';
import { useNotificationsListQuery } from '../api/notifications.queries';
import { useNotificationRealtimeUpdates } from '../hooks/useNotificationRealtimeUpdates';
import { NotificationDropdown } from './NotificationDropdown';

/**
 * The single place responsible for notification UI in this app — mounted
 * once via app/(admin)/layout.tsx's `notificationSlot` (shared/layout can't
 * import this directly, see docs/decisions/0017). Always fetches just page 1
 * (bounded, ~15 items) on mount to compute the unread badge — "lightweight,"
 * never the whole history; the dropdown reuses this same cached page and
 * only loads more when the user scrolls it.
 */
export function NotificationBell() {
  const t = useTranslations('common.notifications');
  const [open, setOpen] = useState(false);
  const list = useNotificationsListQuery();
  const isRead = useNotificationsStore((s) => s.isRead);
  const connectionStatus = useSignalrStore((s) => s.status);
  useNotificationRealtimeUpdates();

  const unreadCount = useMemo(
    () => list.items.filter((item) => !isRead(item.id)).length,
    [list.items, isRead],
  );

  return (
    <AppPopover open={open} onOpenChange={setOpen}>
      <AppPopoverTrigger asChild>
        <IconButton aria-label={t('title')} className="relative">
          <Bell />
          <UnreadBadge count={unreadCount} className="absolute -top-1 -right-1" />
          {connectionStatus === 'connected' ? (
            <span
              aria-hidden
              title={t('connected')}
              className="absolute right-0.5 bottom-0.5 size-1.5 rounded-full bg-emerald-500"
            />
          ) : null}
        </IconButton>
      </AppPopoverTrigger>
      <AppPopoverContent align="end" className="w-80 max-w-[calc(100vw-2rem)] p-2">
        <NotificationDropdown list={list} />
      </AppPopoverContent>
    </AppPopover>
  );
}
