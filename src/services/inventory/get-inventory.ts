import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface GetInventoryResponse {
  id: string;
  productId: string;
  productVariationId: string;
  warehouseId: string;
  quantity: number;
  createdAt: string;
  updatedAt: string;
}

/** GET /inventories/{inventoryId} */
export function getInventory(inventoryId: string): Promise<GetInventoryResponse> {
  return apiClient.get(`${BASE_PATH}/inventories/${inventoryId}`).then((res) => res.data);
}
