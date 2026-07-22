import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface UpdateProductCategoryRequest {
  name: string;
  description?: string;
  /** New parent, or null to move to root. */
  parentCategoryId?: string | null;
}

/** PUT /categories/{categoryId} — can move a category under a different parent. */
export function updateProductCategory(
  categoryId: string,
  request: UpdateProductCategoryRequest,
): Promise<void> {
  return apiClient.put(`${BASE_PATH}/categories/${categoryId}`, request).then(() => undefined);
}
