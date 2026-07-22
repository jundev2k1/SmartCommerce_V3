import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { CartResponse } from './get-cart';

export interface AddCartItemRequest {
  variationId: string;
  quantity: number;
}

/**
 * POST /cart/items — adds to the existing line's quantity if the variation is
 * already in the cart. 404 if the variation doesn't exist, 400 if it exists
 * but isn't currently orderable.
 */
export async function addCartItem(request: AddCartItemRequest): Promise<CartResponse> {
  const res = await apiClient.post(`${BASE_PATH}/cart/items`, request);
  return res.data;
}
