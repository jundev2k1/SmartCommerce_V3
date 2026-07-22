import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface UpdateVariationRequest {
  sku: string;
  price: number;
  /** Free-text status (e.g. "Active"/"Inactive"/"Discontinued"). */
  status?: string;
  barcode?: string;
  cost?: number;
  weight?: number;
  dimensionsLength?: number;
  dimensionsWidth?: number;
  dimensionsHeight?: number;
  images?: string[];
}

/**
 * PUT /products/{productId}/variations/{variationId} — never touches
 * displayOrder or isDefault; use reorder-variations / set-default-variation.
 */
export function updateVariation(
  productId: string,
  variationId: string,
  request: UpdateVariationRequest,
): Promise<void> {
  return apiClient
    .put(`${BASE_PATH}/products/${productId}/variations/${variationId}`, request)
    .then(() => undefined);
}
