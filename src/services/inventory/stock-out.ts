import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface StockOutRequest {
  quantity: number;
  reason: string;
}

export interface StockOutResponse {
  newQuantity: number;
}

/** POST /inventories/{inventoryId}/stock-out */
export function stockOut(inventoryId: string, request: StockOutRequest): Promise<StockOutResponse> {
  return apiClient
    .post(`${BASE_PATH}/inventories/${inventoryId}/stock-out`, request)
    .then((res) => res.data);
}
