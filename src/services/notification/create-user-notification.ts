import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { NotificationPriority } from './types/enums';

export interface CreateUserNotificationCommand {
  userId: string;
  category?: string;
  type?: string;
  title: string;
  body: string;
  priority: NotificationPriority;
  metadataJson?: string;
  expiredAt?: string;
  campaignId?: string;
}

export interface CreateUserNotificationResponse {
  id: string;
}

/**
 * POST /user-notifications — admin only. Creates a Notification Center entry
 * for a user; no automatic rule/campaign trigger is wired up yet.
 */
export function createUserNotification(
  command: CreateUserNotificationCommand,
): Promise<CreateUserNotificationResponse> {
  return apiClient.post(`${BASE_PATH}/user-notifications`, command).then((res) => res.data);
}
