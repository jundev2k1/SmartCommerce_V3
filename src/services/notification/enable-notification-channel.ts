import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

/** POST /notification-channels/{channelId}/enable — requires the configuration to already be Valid. */
export function enableNotificationChannel(channelId: string): Promise<void> {
  return apiClient
    .post(`${BASE_PATH}/notification-channels/${channelId}/enable`)
    .then(() => undefined);
}
