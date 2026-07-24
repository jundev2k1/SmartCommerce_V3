'use client';

import { useEffect, useRef } from 'react';
import { cn } from '@/shared/lib/utils';
import { NotificationItem } from './NotificationItem';
import { NotificationSkeleton } from './NotificationSkeleton';
import { NotificationEmptyState } from './NotificationEmptyState';
import type { UserNotificationSummaryResponse } from '@/services/notification';

export interface NotificationListProps {
  items: UserNotificationSummaryResponse[];
  isRead: (id: string) => boolean;
  onOpen: (notification: UserNotificationSummaryResponse) => void;
  onMarkRead: (id: string) => void;
  isLoading: boolean;
  isError: boolean;
  hasMore: boolean;
  isFetchingMore: boolean;
  onLoadMore: () => void;
  onRetry: () => void;
  /** Group rows by `category` (real field, see docs/modules/notification-center.md) — off by default. */
  groupByCategory?: boolean;
  /** Merged with the default `max-h-96 overflow-y-auto` container (sized for the dropdown) via `cn` — e.g. pass `"max-h-none overflow-visible"` for a full-page list that scrolls with the page itself. */
  className?: string;
}

/**
 * Presentational feed list — loads more automatically once the sentinel row
 * scrolls into view (a real IntersectionObserver, not a fixed timer/preload),
 * satisfying "only load older notifications when users explicitly scroll."
 */
export function NotificationList({
  items,
  isRead,
  onOpen,
  onMarkRead,
  isLoading,
  isError,
  hasMore,
  isFetchingMore,
  onLoadMore,
  onRetry,
  groupByCategory = false,
  className,
}: NotificationListProps) {
  const sentinelRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const node = sentinelRef.current;
    if (!node || !hasMore) return;

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0]?.isIntersecting && !isFetchingMore) {
          onLoadMore();
        }
      },
      { threshold: 1 },
    );
    observer.observe(node);
    return () => observer.disconnect();
  }, [hasMore, isFetchingMore, onLoadMore]);

  if (isLoading) {
    return <NotificationSkeleton />;
  }

  if (isError) {
    return <NotificationEmptyState variant="error" onRetry={onRetry} />;
  }

  if (items.length === 0) {
    return <NotificationEmptyState />;
  }

  const groups = groupByCategory
    ? Object.entries(
        items.reduce<Record<string, UserNotificationSummaryResponse[]>>((acc, item) => {
          const key = item.category ?? '';
          (acc[key] ??= []).push(item);
          return acc;
        }, {}),
      )
    : [['', items] as [string, UserNotificationSummaryResponse[]]];

  return (
    <div className={cn('max-h-96 space-y-3 overflow-y-auto', className)}>
      {groups.map(([category, groupItems]) => (
        <div key={category || 'uncategorized'} className="space-y-1">
          {groupByCategory ? (
            <p className="text-muted-foreground px-2 text-xs font-medium uppercase">
              {category || 'Other'}
            </p>
          ) : null}
          {groupItems.map((item) => (
            <NotificationItem
              key={item.id}
              notification={item}
              isRead={isRead(item.id)}
              onOpen={() => onOpen(item)}
              onMarkRead={() => onMarkRead(item.id)}
            />
          ))}
        </div>
      ))}
      {hasMore ? <div ref={sentinelRef} className="h-2" /> : null}
      {isFetchingMore ? <NotificationSkeleton count={1} /> : null}
    </div>
  );
}
