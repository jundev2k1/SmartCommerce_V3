import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type {
  CampaignExecutionType,
  CampaignStatus,
  NotificationChannelType,
  NotificationPriority,
} from './types/enums';

export interface NotificationCampaignTargetResponse {
  id: string;
  channel: NotificationChannelType;
  templateId: string;
  priority: NotificationPriority;
  enabled: boolean;
}

export interface GetNotificationCampaignResponse {
  id: string;
  name: string | null;
  description: string | null;
  status: CampaignStatus;
  groupId: string;
  executionType: CampaignExecutionType;
  startAt: string;
  endAt: string | null;
  cronExpression: string | null;
  lastExecutedAt: string | null;
  nextExecutionAt: string | null;
  targets: NotificationCampaignTargetResponse[] | null;
  createdAt: string;
}

/** GET /notification-campaigns/{campaignId} */
export function getNotificationCampaign(
  campaignId: string,
): Promise<GetNotificationCampaignResponse> {
  return apiClient.get(`${BASE_PATH}/notification-campaigns/${campaignId}`).then((res) => res.data);
}
