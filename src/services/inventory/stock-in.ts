import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface StockInRequest {
  quantity: number;
  reason: string;
}

export interface StockInResponse {
  newQuantity: number;
}

/** POST /inventories/{inventoryId}/stock-in */
export function stockIn(inventoryId: string, request: StockInRequest): Promise<StockInResponse> {
  return apiClient
    .post(`${BASE_PATH}/inventories/${inventoryId}/stock-in`, request)
    .then((res) => res.data);
}
