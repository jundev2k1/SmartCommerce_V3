import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { OrderStatus } from './types/order-status';

export interface GetOrderItemResponse {
  productId: string;
  productName: string | null;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
  discount?: number;
}

export interface GetOrderResponse {
  id: string;
  customerId: string;
  customerName: string;
  customerPhone: string;
  status: OrderStatus;
  totalAmount: number;
  items: GetOrderItemResponse[] | null;
  shippingAddress?: string;
  cancellationReason?: string;
  createdAt: string;
  updatedAt: string;
}

/** GET /orders/{orderId} */
export async function getOrder(orderId: string): Promise<GetOrderResponse> {
  const res = await apiClient.get(`${BASE_PATH}/orders/${orderId}`);
  return res.data;
}
