import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { ProductCategoryStatus } from './types/product-category-status';

export interface GetProductCategoryResponse {
  id: string;
  code: string | null;
  name: string | null;
  description: string | null;
  status: ProductCategoryStatus;
  parentCategoryId: string | null;
  childCategoryIds: string[] | null;
  createdAt: string;
  updatedAt: string;
}

/** GET /categories/{categoryId} */
export function getProductCategory(categoryId: string): Promise<GetProductCategoryResponse> {
  return apiClient.get(`${BASE_PATH}/categories/${categoryId}`).then((res) => res.data);
}
