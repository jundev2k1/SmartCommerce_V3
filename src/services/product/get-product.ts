import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface ProductVariationResponse {
  id: string;
  sku: string | null;
  barcode: string | null;
  price: number;
  cost: number | null;
  weight: number | null;
  dimensionsLength: number | null;
  dimensionsWidth: number | null;
  dimensionsHeight: number | null;
  images: string[] | null;
  /** Free-text status on the wire (e.g. "Active"/"Inactive"/"Discontinued") — not a numeric enum. */
  status: string | null;
  isDefault: boolean;
  displayOrder: number;
}

export interface GetProductResponse {
  id: string;
  code: string | null;
  name: string | null;
  description: string | null;
  slug: string | null;
  categoryIds: string[] | null;
  tagIds: string[] | null;
  variations: ProductVariationResponse[] | null;
  createdAt: string;
  updatedAt: string;
}

/** GET /products/{productId} */
export function getProduct(productId: string): Promise<GetProductResponse> {
  return apiClient.get(`${BASE_PATH}/products/${productId}`).then((res) => res.data);
}
