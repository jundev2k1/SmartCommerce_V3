import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { OrderStatus } from './types/order-status';

/** No `customerId` — the caller's own auth session is always the identity placing the order. */
export interface CreateOrderRequest {
  customerName: string;
  customerPhone: string;
  items: CreateOrderItemRequestDto[];
}

export interface CreateOrderItemRequestDto {
  productId: string;
  variationId: string;
  quantity: number;
  discount?: number;
}

/** Shared by both `POST /orders` and `POST /orders/admin` — identical response shape. */
export interface CreateOrderResponse {
  orderId: string;
  totalAmount: number;
  status: OrderStatus;
}

/**
 * POST /orders — the caller's own auth session is always the identity
 * placing the order (no `customerId` on the body). `items` must match the
 * caller's current server-side cart exactly — same `(variationId, quantity)`
 * pairs, no more/fewer — or the server responds 409.
 */
export async function createOrder(request: CreateOrderRequest): Promise<CreateOrderResponse> {
  const res = await apiClient.post(`${BASE_PATH}/orders`, request);
  return res.data;
}
