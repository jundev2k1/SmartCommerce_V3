import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { CartResponse } from './get-cart';

/** DELETE /cart/items/{variationId} — no-op (still 200) if the variation wasn't in the cart. */
export async function removeCartItem(variationId: string): Promise<CartResponse> {
  const res = await apiClient.delete(`${BASE_PATH}/cart/items/${variationId}`);
  return res.data;
}
