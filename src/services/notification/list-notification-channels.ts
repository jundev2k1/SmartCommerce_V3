import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type {
  ChannelValidationStatus,
  NotificationChannelStatus,
  NotificationChannelType,
} from './types/enums';

export interface NotificationChannelSummaryResponse {
  id: string;
  channelType: NotificationChannelType;
  displayName: string | null;
  status: NotificationChannelStatus;
  validationStatus: ChannelValidationStatus;
}

/** GET /notification-channels — plain array response, not paginated. */
export function listNotificationChannels(): Promise<NotificationChannelSummaryResponse[]> {
  return apiClient.get(`${BASE_PATH}/notification-channels`).then((res) => res.data);
}
