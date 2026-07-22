'use client';

import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createOrder, type CreateOrderRequest } from '@/services/order';
import { isApiError } from '@/shared/lib/api/types';
import { useLocalOrdersStore } from '@/shared/stores/local-orders.store';
import { cartKeys } from '@/features/cart';

/**
 * The server enforces that `items` matches the caller's current cart exactly,
 * rejecting a mismatch with 409 — invalidate the cart query on that path too
 * (not just on success) so a stale local view doesn't cause a repeat 409.
 * On success the server has already cleared the cart itself; this only
 * invalidates the cached copy so the UI reflects that, per
 * docs/backend/order/README.md.
 */
export function useCreateOrderMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: CreateOrderRequest) => createOrder(request),
    onSuccess: (data) => {
      useLocalOrdersStore.getState().addOrderId(data.orderId);
      queryClient.invalidateQueries({ queryKey: cartKeys.all });
    },
    onError: (error) => {
      if (isApiError(error) && error.status === 409) {
        queryClient.invalidateQueries({ queryKey: cartKeys.all });
      }
    },
  });
}
