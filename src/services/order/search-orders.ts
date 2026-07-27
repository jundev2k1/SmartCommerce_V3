import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { CriteriaRequest } from '@/services/shared/criteria-request';
import type { BackendPaginatedResult } from '@/services/shared/paginated-result';
import type { OrderStatus } from './types/order-status';

export interface SearchOrdersItemResponse {
  id: string;
  customerId: string;
  customerName: string;
  customerPhone: string;
  status: OrderStatus;
  totalAmount: number;
  createdAt: string;
  updatedAt: string;
}

/**
 * POST /orders/search — Admin only. `keyword` free-texts over customerName
 * (the only keyword-searchable field); `filters` allows customerName, phone,
 * status, createdAt; `sorts` allows customerName, status, createdAt (not
 * totalAmount). See Order.Application/Features/Orders/Search/OrderCriteriaDefinition.cs.
 */
export function searchOrders(
  request: CriteriaRequest = {},
): Promise<BackendPaginatedResult<SearchOrdersItemResponse>> {
  return apiClient.post(`${BASE_PATH}/orders/search`, request).then((res) => res.data);
}
