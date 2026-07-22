import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { WarehouseStatus } from './types/warehouse-status';

export interface GetWarehouseResponse {
  id: string;
  code: string | null;
  name: string | null;
  address: string | null;
  status: WarehouseStatus;
  createdAt: string;
  updatedAt: string;
}

/** GET /warehouses/{warehouseId} */
export function getWarehouse(warehouseId: string): Promise<GetWarehouseResponse> {
  return apiClient.get(`${BASE_PATH}/warehouses/${warehouseId}`).then((res) => res.data);
}
