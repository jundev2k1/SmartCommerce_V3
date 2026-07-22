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
 * On receipt, invalidates only that one order's detail query — never the
 * whole order list — since "the list" here is just a set of individual
 * `useOrderQuery`/detail queries (see docs/modules/order-management.md), so a
 * targeted invalidation is already the correct/minimal one.
 */
export function useOrderRealtimeUpdates(onEvent?: (event: OrderStatusChangedEvent) => void) {
  const queryClient = useQueryClient();

  const handler = useCallback(
    (event: OrderStatusChangedEvent) => {
      queryClient.invalidateQueries({ queryKey: orderKeys.detail(event.orderId) });
      onEvent?.(event);
    },
    [queryClient, onEvent],
  );

  useHubEvent<OrderStatusChangedEvent>('OrderStatusChanged', handler);
}
