import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface UpdateUserRequest {
  firstName: string;
  lastName: string;
  phoneNumber: string;
}

/** PUT /profiles/{userId} — response data is an empty object, so this resolves void. */
export function updateUser(userId: string, request: UpdateUserRequest): Promise<void> {
  return apiClient.put(`${BASE_PATH}/profiles/${userId}`, request).then(() => undefined);
}
