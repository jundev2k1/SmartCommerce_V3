import { apiClient } from '@/shared/lib/api/client';
import { IdempotencyOperation } from '@/shared/lib/api/idempotency';
import { BASE_PATH } from './_base';

export interface CreateProductTagRequest {
  code: string;
  name: string;
}

export interface CreateProductTagResponse {
  productTagId: string;
}

/**
 * POST /tags — creates a new flat product tag (no hierarchy). Sends an
 * Idempotency-Key for forward compatibility, same rationale as
 * create-product-category.ts.
 */
export async function createProductTag(
  request: CreateProductTagRequest,
): Promise<CreateProductTagResponse> {
  const res = await apiClient.post(`${BASE_PATH}/tags`, request, {
    idempotency: { operationId: IdempotencyOperation.CreateProductTag },
  });
  return res.data;
}
