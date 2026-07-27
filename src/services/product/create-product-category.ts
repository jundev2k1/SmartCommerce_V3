import { apiClient } from '@/shared/lib/api/client';
import { IdempotencyOperation } from '@/shared/lib/api/idempotency';
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

/**
 * POST /categories — optionally nested under a parent category. Sends an
 * Idempotency-Key for forward compatibility with the backend's opt-in
 * `.RequireIdempotency()` middleware, even though this endpoint doesn't
 * enforce it yet (unlike CreateUser/CreateProduct/CreateOrder) — harmless
 * either way, since the middleware ignores the header on endpoints that
 * aren't marked.
 */
export async function createProductCategory(
  request: CreateProductCategoryRequest,
): Promise<CreateProductCategoryResponse> {
  const res = await apiClient.post(`${BASE_PATH}/categories`, request, {
    idempotency: { operationId: IdempotencyOperation.CreateProductCategory },
  });
  return res.data;
}
