import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { ProductCategoryStatus } from './types/product-category-status';

export interface ProductCategoryItemResponse {
  id: string;
  code: string | null;
  name: string | null;
  status: ProductCategoryStatus;
  parentCategoryId: string | null;
}

export interface ListProductCategoriesResponse {
  categories: ProductCategoryItemResponse[] | null;
}

/** GET /categories — full flat set, not paginated (small reference data). */
export function listProductCategories(): Promise<ListProductCategoriesResponse> {
  return apiClient.get(`${BASE_PATH}/categories`).then((res) => res.data);
}
