import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

/** POST /products/{productId}/variations/{variationId}/default — no-op if already Default. */
export function setDefaultVariation(productId: string, variationId: string): Promise<void> {
  return apiClient
    .post(`${BASE_PATH}/products/${productId}/variations/${variationId}/default`)
    .then(() => undefined);
}
