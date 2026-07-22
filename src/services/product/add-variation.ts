import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { ProductVariationResponse } from './get-product';

/** Shared shape between `AddVariationRequest` and `CreateProductVariationRequest`. */
export interface VariationInput {
  sku: string;
  price: number;
  barcode?: string;
  cost?: number;
  weight?: number;
  dimensionsLength?: number;
  dimensionsWidth?: number;
  dimensionsHeight?: number;
  images?: string[];
}

export interface AddVariationRequest extends VariationInput {
  /** Switches the Default over to this new variation atomically. */
  makeDefault?: boolean;
}

export interface AddVariationResponse {
  variation: ProductVariationResponse;
}

/** POST /products/{productId}/variations — DisplayOrder assigned automatically. */
export function addVariation(
  productId: string,
  request: AddVariationRequest,
): Promise<AddVariationResponse> {
  return apiClient
    .post(`${BASE_PATH}/products/${productId}/variations`, request)
    .then((res) => res.data);
}
