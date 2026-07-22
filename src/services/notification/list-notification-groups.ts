import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { BackendPaginatedResult } from '@/services/shared/paginated-result';
import type { AudienceType, NotificationGroupStatus } from './types/enums';

export interface ListNotificationGroupsParams {
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface NotificationGroupSummaryResponse {
  id: string;
  name: string | null;
  status: NotificationGroupStatus;
  audienceType: AudienceType;
  createdAt: string;
}

/** GET /notification-groups */
export function listNotificationGroups(
  params: ListNotificationGroupsParams = {},
): Promise<BackendPaginatedResult<NotificationGroupSummaryResponse>> {
  return apiClient.get(`${BASE_PATH}/notification-groups`, { params }).then((res) => res.data);
}
