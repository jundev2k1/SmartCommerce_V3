'use client';

import { useTranslations } from 'next-intl';
import { OrderStatusBadge } from './OrderStatusBadge';
import type { OrderStatus } from '@/services/order';

export interface OrderTimelineEvent {
  status: OrderStatus | null | undefined;
  timestamp: string;
  operator?: string;
  message?: string;
}

export interface OrderTimelineProps {
  currentStatus: OrderStatus | null | undefined;
  /**
   * Realtime status-change events observed while this page was open (see
   * features/orders/hooks/useOrderRealtimeUpdates.ts) — `GetOrderResponse`
   * itself has no status-history array, so this is the only real timeline
   * data this contract can ever produce; nothing before the current viewing
   * session is available. See docs/backend/order/README.md and
   * docs/modules/order-management.md.
   */
  events?: OrderTimelineEvent[];
}

/**
 * Generalized so future modules can reuse it for any entity with a
 * status + optional observed-event history — not Order-specific beyond the
 * `OrderStatus` type parameter here.
 */
export function OrderTimeline({ currentStatus, events = [] }: OrderTimelineProps) {
  const t = useTranslations('commerce.orderTimeline');

  return (
    <div className="space-y-3">
      <div className="flex items-center gap-2 text-sm">
        <span className="text-muted-foreground">{t('currentStatus')}:</span>
        <OrderStatusBadge status={currentStatus} />
      </div>

      {events.length === 0 ? (
        <p className="text-muted-foreground rounded-md border border-dashed p-4 text-sm">
          {t('placeholder')}
        </p>
      ) : (
        <ol className="space-y-3 border-l pl-4">
          {events.map((event, index) => (
            <li key={`${event.timestamp}-${index}`} className="space-y-1">
              <div className="flex items-center gap-2">
                <OrderStatusBadge status={event.status} />
                <span className="text-muted-foreground text-sm">
                  {new Date(event.timestamp).toLocaleString()}
                </span>
              </div>
              {event.operator ? (
                <p className="text-sm">
                  {t('operator')}: {event.operator}
                </p>
              ) : null}
              {event.message ? (
                <p className="text-muted-foreground text-sm">{event.message}</p>
              ) : null}
            </li>
          ))}
        </ol>
      )}
    </div>
  );
}
