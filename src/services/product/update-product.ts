import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface UpdateProductRequest {
  name: string;
  description?: string;
  slug: string;
}

/** PUT /products/{productId} — product-level shared info only, never variation data. */
export function updateProduct(productId: string, request: UpdateProductRequest): Promise<void> {
  return apiClient.put(`${BASE_PATH}/products/${productId}`, request).then(() => undefined);
}
