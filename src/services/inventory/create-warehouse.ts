import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface CreateWarehouseRequest {
  code: string;
  name: string;
  address: string;
}

export interface CreateWarehouseResponse {
  warehouseId: string;
}

/** POST /warehouses */
export function createWarehouse(request: CreateWarehouseRequest): Promise<CreateWarehouseResponse> {
  return apiClient.post(`${BASE_PATH}/warehouses`, request).then((res) => res.data);
}
