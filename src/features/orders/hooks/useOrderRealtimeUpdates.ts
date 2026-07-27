'use client';

import { useCallback } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { useHubEvent } from '@/shared/lib/realtime/useHubEvent';
import { orderKeys } from '../api/orders.queries';
import type { OrderStatus } from '@/services/order';

export interface OrderStatusChangedEvent {
  orderId: string;
  status: OrderStatus;
  timestamp: string;
  operator?: string;
  message?: string;
}

/**
 * TODO: `'OrderStatusChanged'` is a placeholder event name — no SignalR event
 * catalog is published by any of the 7 backend services yet (see
 * docs/realtime/signalr-strategy.md "Event catalog" and
 * docs/backend/notification/README.md, which is REST-only with no hub
 * endpoints documented). This proves out the real subscribe -> targeted-
 * invalidate pattern so swapping in the real event name/payload later is a
 * one-line change, not a rearchitecture (per decisions/0007-signalr-strategy.md).
 *
 * On receipt, invalidates that one order's detail query plus every active
 * `useOrdersSearchQuery` result (admin search list/approve queue) — a status
 * change can move an order in or out of a filtered/sorted page, so a
 * targeted per-order invalidation alone isn't enough once a real list query
 * exists (see docs/modules/order-management.md). "My Orders" has no list
 * query to invalidate — it's just individual detail queries, which the first
 * invalidate already covers.
 */
export function useOrderRealtimeUpdates(onEvent?: (event: OrderStatusChangedEvent) => void) {
  const queryClient = useQueryClient();

  const handler = useCallback(
    (event: OrderStatusChangedEvent) => {
      queryClient.invalidateQueries({ queryKey: orderKeys.detail(event.orderId) });
      queryClient.invalidateQueries({ queryKey: orderKeys.searches() });
      onEvent?.(event);
    },
    [queryClient, onEvent],
  );

  useHubEvent<OrderStatusChangedEvent>('OrderStatusChanged', handler);
}
