import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { OrderStatus } from './types/order-status';

export interface CancelOrderResponse {
  orderId: string;
  status: OrderStatus;
}

/** POST /orders/{orderId}/cancel — orders already cancelled/completed cannot be cancelled. */
export async function cancelOrder(orderId: string): Promise<CancelOrderResponse> {
  const res = await apiClient.post(`${BASE_PATH}/orders/${orderId}/cancel`);
  return res.data;
}
