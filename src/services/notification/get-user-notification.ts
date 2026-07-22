import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { NotificationPriority, NotificationStatus } from './types/enums';

export interface GetUserNotificationResponse {
  id: string;
  userId: string;
  category: string | null;
  type: string | null;
  title: string | null;
  body: string | null;
  priority: NotificationPriority;
  status: NotificationStatus;
  readAt: string | null;
  expiredAt: string | null;
  campaignId: string | null;
  createdAt: string;
}

/** GET /user-notifications/{notificationId} — callers may only fetch their own. */
export function getUserNotification(notificationId: string): Promise<GetUserNotificationResponse> {
  return apiClient.get(`${BASE_PATH}/user-notifications/${notificationId}`).then((res) => res.data);
}
