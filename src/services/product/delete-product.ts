import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

/** DELETE /products/{productId} — hard delete, removes all variations too. */
export function deleteProduct(productId: string): Promise<void> {
  return apiClient.delete(`${BASE_PATH}/products/${productId}`).then(() => undefined);
}
