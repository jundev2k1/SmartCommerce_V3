import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { CartResponse } from './get-cart';

export interface UpdateCartItemQuantityRequest {
  quantity: number;
}

/**
 * PATCH /cart/items/{variationId} — 404 if that variation isn't currently in
 * the cart, 400 if quantity <= 0 (rejected rather than treated as a remove).
 */
export async function updateCartItemQuantity(
  variationId: string,
  request: UpdateCartItemQuantityRequest,
): Promise<CartResponse> {
  const res = await apiClient.patch(`${BASE_PATH}/cart/items/${variationId}`, request);
  return res.data;
}
