import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { CriteriaRequest } from '@/services/shared/criteria-request';
import type { BackendPaginatedResult } from '@/services/shared/paginated-result';
import type { InventoryTransactionType } from './types/inventory-transaction-type';

/**
 * Unlike GetInventoryHistory, not scoped to a single inventory id — carries
 * inventoryId/productId/productVariationId/warehouseId per row instead.
 */
export interface SearchInventoryTransactionsItemResponse {
  id: string;
  inventoryId: string;
  productId: string;
  productVariationId: string;
  warehouseId: string;
  type: InventoryTransactionType;
  quantity: number;
  quantityAfter: number;
  reason: string | null;
  createdAt: string;
}

/** POST /inventory-transactions/search — Admin/Root only. */
export function searchInventoryTransactions(
  request: CriteriaRequest = {},
): Promise<BackendPaginatedResult<SearchInventoryTransactionsItemResponse>> {
  return apiClient
    .post(`${BASE_PATH}/inventory-transactions/search`, request)
    .then((res) => res.data);
}
