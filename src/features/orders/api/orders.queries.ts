'use client';

import { useMutation, useQuery, useQueryClient, useQueries } from '@tanstack/react-query';
import { getOrder, cancelOrder, completeOrder, updateOrderOwnerInfo } from '@/services/order';
import { useLocalOrdersStore } from '@/shared/stores/local-orders.store';
import type { UpdateOrderOwnerInfoFormValues } from '../orders.schema';

export const orderKeys = {
  all: ['orders'] as const,
  details: () => [...orderKeys.all, 'detail'] as const,
  detail: (id: string) => [...orderKeys.details(), id] as const,
};

export function useOrderQuery(orderId: string) {
  return useQuery({
    queryKey: orderKeys.detail(orderId),
    queryFn: () => getOrder(orderId),
    enabled: Boolean(orderId),
  });
}

/**
 * "My Orders" has no backing list endpoint (see docs/backend/order/README.md),
 * so it fetches the real GetOrder response for each id tracked in
 * local-orders.store — every field is real backend data, just scoped to
 * orders placed from this browser. See docs/modules/client-mock.md.
 */
export function useLocalOrdersQuery() {
  const orderIds = useLocalOrdersStore((s) => s.orderIds);
  const results = useQueries({
    queries: orderIds.map((id) => ({
      queryKey: orderKeys.detail(id),
      queryFn: () => getOrder(id),
    })),
  });

  return {
    orders: results.map((r) => r.data).filter((order) => order !== undefined),
    isLoading: results.some((r) => r.isLoading),
    hasAny: orderIds.length > 0,
  };
}

export function useCancelOrderMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (orderId: string) => cancelOrder(orderId),
    onSuccess: (_data, orderId) =>
      queryClient.invalidateQueries({ queryKey: orderKeys.detail(orderId) }),
  });
}

/** Admin-only — POST /orders/{orderId}/complete, only valid on a Confirmed order. */
export function useCompleteOrderMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (orderId: string) => completeOrder(orderId),
    onSuccess: (_data, orderId) =>
      queryClient.invalidateQueries({ queryKey: orderKeys.detail(orderId) }),
  });
}

/** PATCH /orders/{orderId}/owner-info — only valid while the order is Pending or Confirmed. */
export function useUpdateOrderOwnerInfoMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      orderId,
      values,
    }: {
      orderId: string;
      values: UpdateOrderOwnerInfoFormValues;
    }) => updateOrderOwnerInfo(orderId, values),
    onSuccess: (_data, { orderId }) =>
      queryClient.invalidateQueries({ queryKey: orderKeys.detail(orderId) }),
  });
}
