import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

/**
 * DELETE /products/{productId}/variations/{variationId} — the last remaining
 * variation of a product can never be removed. Removing the current Default
 * auto-promotes the remaining variation with the lowest displayOrder.
 */
export function deleteVariation(productId: string, variationId: string): Promise<void> {
  return apiClient
    .delete(`${BASE_PATH}/products/${productId}/variations/${variationId}`)
    .then(() => undefined);
}
