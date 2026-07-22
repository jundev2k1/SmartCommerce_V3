import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

/** POST /products/{productId}/tags/{tagId} — idempotent. */
export function assignProductTag(productId: string, tagId: string): Promise<void> {
  return apiClient.post(`${BASE_PATH}/products/${productId}/tags/${tagId}`).then(() => undefined);
}
