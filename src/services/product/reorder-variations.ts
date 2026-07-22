import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface ReorderVariationsRequest {
  /** Every existing variation id, each exactly once, in the desired display order. */
  orderedVariationIds: string[];
}

/** POST /products/{productId}/variations/reorder */
export function reorderVariations(
  productId: string,
  request: ReorderVariationsRequest,
): Promise<void> {
  return apiClient
    .post(`${BASE_PATH}/products/${productId}/variations/reorder`, request)
    .then(() => undefined);
}
