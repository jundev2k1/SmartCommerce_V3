import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { UserStatus } from './types/user-status';

export interface GetUserResponse {
  id: string;
  email: string | null;
  userName: string | null;
  phoneNumber: string | null;
  firstName: string | null;
  lastName: string | null;
  status: UserStatus;
  createdAt: string;
  updatedAt: string;
}

/** GET /profiles/{userId} */
export function getUser(userId: string): Promise<GetUserResponse> {
  return apiClient.get(`${BASE_PATH}/profiles/${userId}`).then((res) => res.data);
}
