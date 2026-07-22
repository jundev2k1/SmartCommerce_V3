import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { CreateOrderItemRequestDto, CreateOrderResponse } from './create-order';

/** `customerId` is required here (unlike `CreateOrderRequest`) since there's no cart to source it from. */
export interface AdminCreateOrderRequest {
  customerId: string;
  customerName: string;
  customerPhone: string;
  items: CreateOrderItemRequestDto[];
}

/**
 * POST /orders/admin — creates an order on behalf of a specified customer.
 * Requires `Admin`/`Root` (same `RequireAdmin` policy as `CompleteOrder`); no
 * cart-diff check applies since there's no cart to source items from.
 */
export async function adminCreateOrder(
  request: AdminCreateOrderRequest,
): Promise<CreateOrderResponse> {
  const res = await apiClient.post(`${BASE_PATH}/orders/admin`, request);
  return res.data;
}
