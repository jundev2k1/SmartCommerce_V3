import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { CriteriaRequest } from '@/services/shared/criteria-request';
import type { BackendPaginatedResult } from '@/services/shared/paginated-result';
import type { GetInventoryResponse } from './get-inventory';

/** POST /inventories/search — Admin/Root only. Item shape matches GetInventoryResponse exactly. */
export function searchInventories(
  request: CriteriaRequest = {},
): Promise<BackendPaginatedResult<GetInventoryResponse>> {
  return apiClient.post(`${BASE_PATH}/inventories/search`, request).then((res) => res.data);
}
