import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { InventoryTransactionType } from './types/inventory-transaction-type';

export interface InventoryTransactionDto {
  id: string;
  type: InventoryTransactionType;
  quantity: number;
  quantityAfter: number;
  reason: string | null;
  createdAt: string;
}

export interface GetInventoryHistoryResponse {
  transactions: InventoryTransactionDto[] | null;
}

/** GET /inventories/{inventoryId}/history */
export function getInventoryHistory(inventoryId: string): Promise<GetInventoryHistoryResponse> {
  return apiClient.get(`${BASE_PATH}/inventories/${inventoryId}/history`).then((res) => res.data);
}
