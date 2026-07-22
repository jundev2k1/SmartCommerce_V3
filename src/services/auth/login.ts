import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface LoginRequest {
  email: string;
  password: string;
}

/**
 * POST /login — sets HTTP-only AccessToken/RefreshToken cookies.
 * No tokens in the response body; `data` is always null.
 */
export async function login(request: LoginRequest): Promise<void> {
  await apiClient.post(`${BASE_PATH}/login`, request);
  return undefined;
}
