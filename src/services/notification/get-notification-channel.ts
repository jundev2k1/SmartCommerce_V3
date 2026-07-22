import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type {
  ChannelValidationStatus,
  NotificationChannelStatus,
  NotificationChannelType,
} from './types/enums';

export interface GetNotificationChannelResponse {
  id: string;
  channelType: NotificationChannelType;
  displayName: string | null;
  status: NotificationChannelStatus;
  configJson: string | null;
  validationStatus: ChannelValidationStatus;
  lastValidatedAt: string | null;
  lastValidationError: string | null;
}

/** GET /notification-channels/{channelId} */
export function getNotificationChannel(channelId: string): Promise<GetNotificationChannelResponse> {
  return apiClient.get(`${BASE_PATH}/notification-channels/${channelId}`).then((res) => res.data);
}
