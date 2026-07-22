import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
}

export interface RegisterResponse {
  userId: string;
}

/**
 * POST /register — sets HTTP-only AccessToken/RefreshToken cookies.
 * Swagger marks `X-Correlation-Id` as a required header here; the shared Axios
 * client already attaches it to every request, so no special handling is needed.
 */
export async function register(request: RegisterRequest): Promise<RegisterResponse> {
  const res = await apiClient.post(`${BASE_PATH}/register`, request);
  return res.data;
}
