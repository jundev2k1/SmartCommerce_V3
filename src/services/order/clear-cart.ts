import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

/** DELETE /cart — empties the cart entirely. */
export async function clearCart(): Promise<void> {
  await apiClient.delete(`${BASE_PATH}/cart`);
  return undefined;
}
