'use client';

import { useTranslations } from 'next-intl';
import { Check } from 'lucide-react';
import { IconButton } from '@/shared/ui';
import { NotificationIcon } from './NotificationIcon';
import { NotificationTimestamp } from './NotificationTimestamp';
import { cn } from '@/shared/lib/utils';
import type { UserNotificationSummaryResponse } from '@/services/notification';

export interface NotificationItemProps {
  notification: UserNotificationSummaryResponse;
  isRead: boolean;
  onOpen: () => void;
  onMarkRead: () => void;
}

/**
 * Purely presentational — read/unread comes from the caller (see
 * shared/stores/notifications.store.ts's `readIds`, since the backend
 * doesn't expose a read/unread signal on this DTO shape). Clicking the row
 * opens the detail dialog; the small check button marks read without
 * opening anything.
 */
export function NotificationItem({
  notification,
  isRead,
  onOpen,
  onMarkRead,
}: NotificationItemProps) {
  const t = useTranslations('notificationsUi.item');

  return (
    <div
      className={cn(
        'hover:bg-accent flex items-start gap-3 rounded-md p-2 text-sm',
        !isRead && 'bg-accent/40',
      )}
    >
      <button type="button" onClick={onOpen} className="flex flex-1 items-start gap-3 text-left">
        <NotificationIcon
          category={notification.category}
          className="text-muted-foreground mt-0.5 size-4 shrink-0"
        />
        <div className="min-w-0 flex-1 space-y-0.5">
          <p className={cn('truncate', !isRead && 'font-medium')}>
            {notification.title ?? t('untitled')}
          </p>
          <NotificationTimestamp
            value={notification.createdAt}
            className="text-muted-foreground text-xs"
          />
        </div>
      </button>
      {!isRead ? (
        <IconButton aria-label={t('markRead')} onClick={onMarkRead}>
          <Check className="size-4" />
        </IconButton>
      ) : null}
    </div>
  );
}
