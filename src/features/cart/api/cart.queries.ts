'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  getCart,
  addCartItem,
  updateCartItemQuantity,
  removeCartItem,
  clearCart,
} from '@/services/order';

export const cartKeys = {
  all: ['cart'] as const,
};

/**
 * Cart is real server state now (Redis-backed on the Order service) — no
 * Zustand involvement, unlike the old client-only `cart.store.ts`. Prices can
 * legitimately change between two calls with no mutation in between, since
 * every read re-resolves against the live product catalog.
 */
export function useCartQuery() {
  return useQuery({
    queryKey: cartKeys.all,
    queryFn: getCart,
  });
}

function invalidateCart(queryClient: ReturnType<typeof useQueryClient>) {
  return queryClient.invalidateQueries({ queryKey: cartKeys.all });
}

export function useAddCartItemMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ variationId, quantity }: { variationId: string; quantity: number }) =>
      addCartItem({ variationId, quantity }),
    onSuccess: () => invalidateCart(queryClient),
  });
}

export function useUpdateCartItemQuantityMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ variationId, quantity }: { variationId: string; quantity: number }) =>
      updateCartItemQuantity(variationId, { quantity }),
    onSuccess: () => invalidateCart(queryClient),
  });
}

export function useRemoveCartItemMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (variationId: string) => removeCartItem(variationId),
    onSuccess: () => invalidateCart(queryClient),
  });
}

export function useClearCartMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => clearCart(),
    onSuccess: () => invalidateCart(queryClient),
  });
}
