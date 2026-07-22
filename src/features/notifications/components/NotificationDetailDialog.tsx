'use client';

import { useEffect } from 'react';
import { useTranslations } from 'next-intl';
import { AppModal, AppLoading, AppEmpty } from '@/shared/ui';
import { EntityMetadata } from '@/shared/entity';
import { NotificationTimestamp } from '@/shared/notifications';
import { useNotificationQuery, useMarkAsReadMutation } from '../api/notifications.queries';
import { useNotificationsStore } from '@/shared/stores/notifications.store';

export interface NotificationDetailDialogProps {
  notificationId: string | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

/**
 * Opening this dialog marks the notification read — same convention as
 * reading an email. "Related entity"/"Navigation action" from the brief are
 * limited to a plain `campaignId` value with no link target, since there's
 * no generic entity-reference field on `GetUserNotificationResponse` and no
 * Campaign detail page exists yet (reserved nav placeholder only — see
 * docs/modules/notification-center.md).
 */
export function NotificationDetailDialog({
  notificationId,
  open,
  onOpenChange,
}: NotificationDetailDialogProps) {
  const t = useTranslations('notifications.detail');
  const { data: notification, isLoading } = useNotificationQuery(notificationId ?? '');
  const markAsReadMutation = useMarkAsReadMutation();
  const isRead = useNotificationsStore((s) => s.isRead);

  useEffect(() => {
    if (open && notificationId && !isRead(notificationId)) {
      markAsReadMutation.mutate(notificationId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps -- only re-run when the dialog opens for a (possibly new) notification, not on every mutation-object identity change
  }, [open, notificationId]);

  return (
    <AppModal open={open} onOpenChange={onOpenChange} title={notification?.title ?? t('title')}>
      {isLoading ? (
        <AppLoading />
      ) : !notification ? (
        <AppEmpty title={t('notFound')} />
      ) : (
        <div className="space-y-3">
          <p className="text-sm">{notification.body}</p>
          <NotificationTimestamp
            value={notification.createdAt}
            className="text-muted-foreground text-xs"
          />
          <EntityMetadata
            items={[
              { label: t('metadata.category'), value: notification.category ?? '—' },
              { label: t('metadata.type'), value: notification.type ?? '—' },
              { label: t('metadata.priority'), value: String(notification.priority) },
              { label: t('metadata.campaignId'), value: notification.campaignId ?? '—' },
              {
                label: t('metadata.expiredAt'),
                value: notification.expiredAt
                  ? new Date(notification.expiredAt).toLocaleString()
                  : '—',
              },
            ]}
          />
        </div>
      )}
    </AppModal>
  );
}
