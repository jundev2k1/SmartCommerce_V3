import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface CreateProductCategoryRequest {
  code: string;
  name: string;
  description?: string;
  parentCategoryId?: string;
}

export interface CreateProductCategoryResponse {
  productCategoryId: string;
}

/** POST /categories — optionally nested under a parent category. */
export function createProductCategory(
  request: CreateProductCategoryRequest,
): Promise<CreateProductCategoryResponse> {
  return apiClient.post(`${BASE_PATH}/categories`, request).then((res) => res.data);
}
