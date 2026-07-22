'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import { NotificationList, NotificationActions } from '@/shared/notifications';
import { useNotificationsStore } from '@/shared/stores/notifications.store';
import { useNotificationsListQuery, useMarkAsReadMutation } from '../api/notifications.queries';
import { NotificationDetailDialog } from './NotificationDetailDialog';

export interface NotificationDropdownProps {
  list: ReturnType<typeof useNotificationsListQuery>;
}

/**
 * Reusable dropdown body — takes the already-fetched list query result as a
 * prop from NotificationBell (same query key, so no duplicate fetch) rather
 * than calling the hook again itself.
 */
export function NotificationDropdown({ list }: NotificationDropdownProps) {
  const t = useTranslations('notifications');
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
    <div className="space-y-2">
      <div className="flex items-center justify-between px-1">
        <p className="text-sm font-medium">{t('title')}</p>
        <NotificationActions
          onMarkAllRead={handleMarkAllRead}
          markingAllRead={markingAll}
          disabled={unreadLoaded.length === 0}
        />
      </div>

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
      />

      <NotificationDetailDialog
        notificationId={detailId}
        open={detailId !== null}
        onOpenChange={(open) => !open && setDetailId(null)}
      />
    </div>
  );
}
