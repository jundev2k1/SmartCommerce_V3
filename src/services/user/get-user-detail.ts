import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { UserStatus } from './types/user-status';

export interface GetUserDetailResponse {
  id: string;
  email: string | null;
  userName: string | null;
  phoneNumber: string | null;
  firstName: string | null;
  lastName: string | null;
  status: UserStatus;
  /** Cached by the Auth service for this user; empty list if nothing cached yet. */
  roles: string[] | null;
  createdAt: string;
  updatedAt: string;
}

/**
 * GET /profiles/current/detail — no path/query parameters in the actual
 * Swagger route (derives the current user from the auth session). The
 * endpoint's prose description mentions a `userId` route parameter that
 * doesn't exist on this route; likely a stale copy-paste from GetUserEndpoint.
 * See docs/backend/user/README.md "Known limitations."
 */
export function getUserDetail(): Promise<GetUserDetailResponse> {
  return apiClient.get(`${BASE_PATH}/profiles/current/detail`).then((res) => res.data);
}
