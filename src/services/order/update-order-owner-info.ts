import { apiClient } from '@/shared/lib/api/client';
import { IdempotencyOperation } from '@/shared/lib/api/idempotency';
import { BASE_PATH } from './_base';

export interface UpdateOrderOwnerInfoRequest {
  customerPhone: string;
  shippingAddress: string;
}

export interface UpdateOrderOwnerInfoResponse {
  orderId: string;
  customerPhone: string;
  shippingAddress: string;
}

/**
 * PATCH /orders/{orderId}/owner-info — updates the shipping address/contact
 * phone snapshot on an order. Only the order's own customer or an admin may
 * call this; only while the order is Pending or Confirmed. Requires an
 * Idempotency-Key (backend `.RequireIdempotency()`).
 */
export async function updateOrderOwnerInfo(
  orderId: string,
  request: UpdateOrderOwnerInfoRequest,
): Promise<UpdateOrderOwnerInfoResponse> {
  const res = await apiClient.patch(`${BASE_PATH}/orders/${orderId}/owner-info`, request, {
    idempotency: { operationId: IdempotencyOperation.UpdateOrderOwnerInfo },
  });
  return res.data;
}
