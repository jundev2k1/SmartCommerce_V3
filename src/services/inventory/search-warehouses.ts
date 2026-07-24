import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { CriteriaRequest } from '@/services/shared/criteria-request';
import type { BackendPaginatedResult } from '@/services/shared/paginated-result';
import type { GetWarehouseResponse } from './get-warehouse';

/** POST /warehouses/search — Admin/Root only. Item shape matches GetWarehouseResponse exactly. */
export function searchWarehouses(
  request: CriteriaRequest = {},
): Promise<BackendPaginatedResult<GetWarehouseResponse>> {
  return apiClient.post(`${BASE_PATH}/warehouses/search`, request).then((res) => res.data);
}
