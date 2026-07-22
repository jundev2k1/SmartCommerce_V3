import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

/** DELETE /categories/{categoryId} — refuses if it has children or is still assigned to a product. */
export function deleteProductCategory(categoryId: string): Promise<void> {
  return apiClient.delete(`${BASE_PATH}/categories/${categoryId}`).then(() => undefined);
}
