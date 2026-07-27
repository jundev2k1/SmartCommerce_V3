import { apiClient } from '@/shared/lib/api/client';
import { IdempotencyOperation } from '@/shared/lib/api/idempotency';
import { BASE_PATH } from './_base';

export interface CreateUserRequest {
  email: string;
  userName: string;
  phoneNumber: string;
  firstName: string;
  lastName: string;
  /** Root may grant Admin or User; Admin may only grant User. */
  roles?: string[];
  /** Initial password; user should be required to change it on first login. */
  tempPassword?: string;
}

export interface CreateUserResponse {
  userId: string;
}

/**
 * POST /profiles — requires X-Correlation-Id (sent globally, see
 * services/api-layer.md) and an Idempotency-Key (backend `.RequireIdempotency()`).
 */
export async function createUser(request: CreateUserRequest): Promise<CreateUserResponse> {
  const res = await apiClient.post(`${BASE_PATH}/profiles`, request, {
    idempotency: { operationId: IdempotencyOperation.CreateUser },
  });
  return res.data;
}
