import { apiClient } from '@/shared/lib/api/client';
import { IdempotencyOperation } from '@/shared/lib/api/idempotency';
import { BASE_PATH } from './_base';
import type { VariationInput } from './add-variation';

export interface CreateProductVariationRequest extends VariationInput {
  isDefault?: boolean;
}

export interface CreateProductRequest {
  code: string;
  name: string;
  description?: string;
  slug: string;
  /** At least one variation required. If none is marked isDefault, the first is used. */
  variations: CreateProductVariationRequest[];
  categoryIds?: string[];
  tagIds?: string[];
}

export interface CreateProductResponse {
  productId: string;
  defaultVariationId: string;
}

/**
 * POST /products — creates a product together with all initial variations in
 * one request (aggregate initialization). Use the dedicated variation
 * endpoints afterward for add/update/remove/reorder. Requires an
 * Idempotency-Key (backend `.RequireIdempotency()`).
 */
export async function createProduct(request: CreateProductRequest): Promise<CreateProductResponse> {
  const res = await apiClient.post(`${BASE_PATH}/products`, request, {
    idempotency: { operationId: IdempotencyOperation.CreateProduct },
  });
  return res.data;
}
