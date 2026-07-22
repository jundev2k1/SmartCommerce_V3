import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

/**
 * POST /refresh-token — refresh-token cookie required, sets new access/refresh cookies.
 * Exposed here for completeness; the 401-retry flow in shared/lib/api/client.ts calls
 * this same path directly (a literal, not this function) since `shared/lib` can never
 * import from `services/` — see docs/architecture/overview.md's dependency direction.
 */
export async function refreshToken(): Promise<void> {
  await apiClient.post(`${BASE_PATH}/refresh-token`);
  return undefined;
}
