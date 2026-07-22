import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface AdjustStockRequest {
  newQuantity: number;
  reason: string;
}

export interface AdjustStockResponse {
  newQuantity: number;
}

/** POST /inventories/{inventoryId}/adjust — direct correction (e.g. physical count). */
export function adjustStock(
  inventoryId: string,
  request: AdjustStockRequest,
): Promise<AdjustStockResponse> {
  return apiClient
    .post(`${BASE_PATH}/inventories/${inventoryId}/adjust`, request)
    .then((res) => res.data);
}
