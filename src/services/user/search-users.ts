import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { CriteriaRequest } from '@/services/shared/criteria-request';
import type { BackendPaginatedResult } from '@/services/shared/paginated-result';
import type { UserStatus } from './types/user-status';

export interface SearchUsersItemResponse {
  id: string;
  email: string | null;
  userName: string | null;
  phoneNumber: string | null;
  firstName: string | null;
  lastName: string | null;
  status: UserStatus;
  /** Write-once snapshot taken at profile-creation time — not live (no role-change endpoint exists yet). */
  roles: string[] | null;
  createdAt: string;
  updatedAt: string;
}

/**
 * POST /users/search — Admin/Root only. Supports keyword search over
 * userName/email/firstName/lastName/phone, sorting on userName/email/status/
 * createdAt, and a `role` filter (`eq`/`ne` only, not sortable — role is
 * multi-valued, matched via array-contains on the backend).
 */
export function searchUsers(
  request: CriteriaRequest = {},
): Promise<BackendPaginatedResult<SearchUsersItemResponse>> {
  return apiClient.post(`${BASE_PATH}/users/search`, request).then((res) => res.data);
}
