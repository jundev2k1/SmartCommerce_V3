import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { CursorPaginatedResult } from '@/services/shared/paginated-result';
import type { NotificationPriority, NotificationStatus } from './types/enums';

export interface ListMyUserNotificationsParams {
  status?: NotificationStatus;
  cursor?: string;
  limit?: number;
}

export interface UserNotificationSummaryResponse {
  id: string;
  category: string | null;
  type: string | null;
  title: string | null;
  priority: NotificationPriority;
  status: NotificationStatus;
  createdAt: string;
}

/** GET /user-notifications/me — the caller's own Notification Center entries. */
export function listMyUserNotifications(
  params: ListMyUserNotificationsParams = {},
): Promise<CursorPaginatedResult<UserNotificationSummaryResponse>> {
  return apiClient.get(`${BASE_PATH}/user-notifications/me`, { params }).then((res) => res.data);
}
