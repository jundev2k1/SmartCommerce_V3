import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface CartItemResponse {
  productId: string;
  variationId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
}

/**
 * Never a stale snapshot — every read re-resolves name/price/availability from
 * the product catalog; a line whose variation became unavailable is silently
 * dropped before the response is built (not returned, not errored).
 */
export interface CartResponse {
  items: CartItemResponse[];
  totalAmount: number;
}

/** GET /cart — the user is resolved server-side from the auth session, never passed explicitly. */
export async function getCart(): Promise<CartResponse> {
  const res = await apiClient.get(`${BASE_PATH}/cart`);
  return res.data;
}
