import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface GetProductStockParams {
  productVariationId?: string;
}

export interface GetProductStockResponse {
  productId: string;
  productVariationId: string | null;
  totalQuantity: number;
}

/**
 * GET /products/{productId}/stock — rolls up every variation/warehouse unless
 * `productVariationId` narrows it to one variation. Backs the same query the
 * Order Service's gRPC stock lookup uses.
 */
export function getProductStock(
  productId: string,
  params: GetProductStockParams = {},
): Promise<GetProductStockResponse> {
  return apiClient
    .get(`${BASE_PATH}/products/${productId}/stock`, { params })
    .then((res) => res.data);
}
