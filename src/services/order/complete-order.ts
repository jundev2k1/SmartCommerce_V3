import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { OrderStatus } from './types/order-status';

export interface CompleteOrderResponse {
  orderId: string;
  status: OrderStatus;
}

/** POST /orders/{orderId}/complete — admin-only; only a Confirmed order can be completed. */
export async function completeOrder(orderId: string): Promise<CompleteOrderResponse> {
  const res = await apiClient.post(`${BASE_PATH}/orders/${orderId}/complete`);
  return res.data;
}
