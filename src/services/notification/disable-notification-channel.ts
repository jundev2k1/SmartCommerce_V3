import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

/** POST /notification-channels/{channelId}/disable */
export function disableNotificationChannel(channelId: string): Promise<void> {
  return apiClient
    .post(`${BASE_PATH}/notification-channels/${channelId}/disable`)
    .then(() => undefined);
}
