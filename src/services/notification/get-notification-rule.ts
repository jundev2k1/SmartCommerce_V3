import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type {
  NotificationChannelType,
  NotificationPriority,
  NotificationRuleStatus,
} from './types/enums';

export interface NotificationRuleTargetResponse {
  id: string;
  channel: NotificationChannelType;
  templateId: string;
  priority: NotificationPriority;
  enabled: boolean;
}

export interface GetNotificationRuleResponse {
  id: string;
  name: string | null;
  description: string | null;
  eventType: string | null;
  status: NotificationRuleStatus;
  targets: NotificationRuleTargetResponse[] | null;
  createdAt: string;
}

/** GET /notification-rules/{ruleId} */
export function getNotificationRule(ruleId: string): Promise<GetNotificationRuleResponse> {
  return apiClient.get(`${BASE_PATH}/notification-rules/${ruleId}`).then((res) => res.data);
}
