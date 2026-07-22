import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

/** DELETE /products/{productId}/categories/{categoryId} — idempotent. */
export function removeProductCategory(productId: string, categoryId: string): Promise<void> {
  return apiClient
    .delete(`${BASE_PATH}/products/${productId}/categories/${categoryId}`)
    .then(() => undefined);
}
