import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

/** POST /user-notifications/{notificationId}/read — one of the caller's own entries. */
export function markUserNotificationAsRead(notificationId: string): Promise<void> {
  return apiClient
    .post(`${BASE_PATH}/user-notifications/${notificationId}/read`)
    .then(() => undefined);
}
