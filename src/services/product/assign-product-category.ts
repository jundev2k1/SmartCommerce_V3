import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

/** POST /products/{productId}/categories/{categoryId} — idempotent. */
export function assignProductCategory(productId: string, categoryId: string): Promise<void> {
  return apiClient
    .post(`${BASE_PATH}/products/${productId}/categories/${categoryId}`)
    .then(() => undefined);
}
