'use client';

import { useTranslations } from 'next-intl';
import { SecondaryButton } from '@/shared/ui';

export interface NotificationActionsProps {
  onMarkAllRead?: () => void;
  markingAllRead?: boolean;
  disabled?: boolean;
}

/**
 * "Mark all as read" is composed client-side from the real per-item
 * `MarkUserNotificationAsRead` endpoint — no bulk endpoint exists on this
 * service (see docs/backend/notification/README.md), so this only ever
 * marks notifications currently loaded into the client, not the caller's
 * entire history. See docs/modules/notification-center.md.
 */
export function NotificationActions({
  onMarkAllRead,
  markingAllRead,
  disabled,
}: NotificationActionsProps) {
  const t = useTranslations('notificationsUi.actions');

  if (!onMarkAllRead) {
    return null;
  }

  return (
    <SecondaryButton onClick={onMarkAllRead} loading={markingAllRead} disabled={disabled}>
      {t('markAllRead')}
    </SecondaryButton>
  );
}
