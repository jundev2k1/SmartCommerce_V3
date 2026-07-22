import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { BackendPaginatedResult } from '@/services/shared/paginated-result';
import type { DispatchStatus, NotificationChannelType } from './types/enums';

export interface ListNotificationDispatchesParams {
  status?: DispatchStatus;
  page?: number;
  pageSize?: number;
}

export interface NotificationDispatchSummaryResponse {
  id: string;
  referenceType: string | null;
  referenceId: string | null;
  channel: NotificationChannelType;
  status: DispatchStatus;
  retryCount: number;
  createdAt: string;
}

/** GET /notification-dispatches */
export function listNotificationDispatches(
  params: ListNotificationDispatchesParams = {},
): Promise<BackendPaginatedResult<NotificationDispatchSummaryResponse>> {
  return apiClient.get(`${BASE_PATH}/notification-dispatches`, { params }).then((res) => res.data);
}
