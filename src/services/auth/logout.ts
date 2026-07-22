import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

/** POST /logout — clears session tokens. Requires an authenticated session. */
export async function logout(): Promise<void> {
  await apiClient.post(`${BASE_PATH}/logout`);
  return undefined;
}
