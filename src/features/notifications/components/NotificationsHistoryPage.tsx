'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import { EntityHeader } from '@/shared/entity';
import { NotificationList, NotificationActions } from '@/shared/notifications';
import { useNotificationsStore } from '@/shared/stores/notifications.store';
import { useNotificationsListQuery, useMarkAsReadMutation } from '../api/notifications.queries';
import { NotificationDetailDialog } from './NotificationDetailDialog';

/**
 * Full-history counterpart to NotificationBell/NotificationDropdown (the
 * "lightweight, bounded" topbar experience) — same query key, same
 * generically-built shared/notifications components, just a page instead of
 * a popover with no height cap. Realtime updates are already handled
 * globally by NotificationBell's useNotificationRealtimeUpdates() (mounted
 * once in the admin layout), so this page doesn't call it again.
 */
export function NotificationsHistoryPage() {
  const t = useTranslations('notifications');
  const list = useNotificationsListQuery();
  const isRead = useNotificationsStore((s) => s.isRead);
  const markAsReadMutation = useMarkAsReadMutation();
  const [detailId, setDetailId] = useState<string | null>(null);
  const [markingAll, setMarkingAll] = useState(false);

  const unreadLoaded = list.items.filter((item) => !isRead(item.id));

  async function handleMarkAllRead() {
    setMarkingAll(true);
    try {
      await Promise.all(unreadLoaded.map((item) => markAsReadMutation.mutateAsync(item.id)));
    } finally {
      setMarkingAll(false);
    }
  }

  return (
    <div className="space-y-4">
      <EntityHeader
        title={t('title')}
        description={t('historyDescription')}
        actions={
          <NotificationActions
            onMarkAllRead={handleMarkAllRead}
            markingAllRead={markingAll}
            disabled={unreadLoaded.length === 0}
          />
        }
      />

      <NotificationList
        items={list.items}
        isRead={isRead}
        onOpen={(notification) => setDetailId(notification.id)}
        onMarkRead={(id) => markAsReadMutation.mutate(id)}
        isLoading={list.isLoading}
        isError={list.isError}
        hasMore={list.hasMore}
        isFetchingMore={list.isFetchingMore}
        onLoadMore={list.loadMore}
        onRetry={list.refetch}
        groupByCategory
        className="max-h-none overflow-visible"
      />

      <NotificationDetailDialog
        notificationId={detailId}
        open={detailId !== null}
        onOpenChange={(open) => !open && setDetailId(null)}
      />
    </div>
  );
}
