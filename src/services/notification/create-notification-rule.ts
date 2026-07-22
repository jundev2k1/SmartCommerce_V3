import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { NotificationChannelType, NotificationPriority } from './types/enums';

export interface NotificationRuleTargetInput {
  channel: NotificationChannelType;
  templateId: string;
  priority: NotificationPriority;
}

export interface CreateNotificationRuleCommand {
  name: string;
  description?: string;
  /** Business event name that triggers this rule, e.g. "OrderCreated". */
  eventType: string;
  targets?: NotificationRuleTargetInput[];
}

export interface CreateNotificationRuleResponse {
  id: string;
}

/**
 * POST /notification-rules — defines what notification actions to create when
 * a business event occurs (e.g. OrderCreated -> User Notification + Email + Telegram).
 */
export function createNotificationRule(
  command: CreateNotificationRuleCommand,
): Promise<CreateNotificationRuleResponse> {
  return apiClient.post(`${BASE_PATH}/notification-rules`, command).then((res) => res.data);
}
