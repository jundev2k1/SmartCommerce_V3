import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type {
  CampaignExecutionType,
  NotificationChannelType,
  NotificationPriority,
} from './types/enums';

export interface NotificationCampaignTargetInput {
  channel: NotificationChannelType;
  templateId: string;
  priority: NotificationPriority;
}

export interface CreateNotificationCampaignCommand {
  name: string;
  description?: string;
  groupId: string;
  executionType: CampaignExecutionType;
  startAt: string;
  endAt?: string;
  /** Required if `executionType` is recurring. */
  cronExpression?: string;
  targets?: NotificationCampaignTargetInput[];
}

export interface CreateNotificationCampaignResponse {
  id: string;
}

/**
 * POST /notification-campaigns — starts in Draft; call activate separately
 * once execution is implemented (not yet exposed by this contract).
 */
export function createNotificationCampaign(
  command: CreateNotificationCampaignCommand,
): Promise<CreateNotificationCampaignResponse> {
  return apiClient.post(`${BASE_PATH}/notification-campaigns`, command).then((res) => res.data);
}
