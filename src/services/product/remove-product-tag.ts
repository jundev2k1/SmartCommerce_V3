import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

/** DELETE /products/{productId}/tags/{tagId} — idempotent. */
export function removeProductTag(productId: string, tagId: string): Promise<void> {
  return apiClient.delete(`${BASE_PATH}/products/${productId}/tags/${tagId}`).then(() => undefined);
}
