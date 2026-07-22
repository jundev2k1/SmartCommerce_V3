import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface UpdateChannelConfigurationRequest {
  /** Free-form JSON string (SMTP host, bot token, ...); shape depends on channel type. */
  configJson: string;
}

/** PUT /notification-channels/{channelId}/configuration — resets validationStatus to NotValidated. */
export function updateNotificationChannelConfiguration(
  channelId: string,
  request: UpdateChannelConfigurationRequest,
): Promise<void> {
  return apiClient
    .put(`${BASE_PATH}/notification-channels/${channelId}/configuration`, request)
    .then(() => undefined);
}
