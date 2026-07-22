import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { DispatchStatus, NotificationChannelType } from './types/enums';

export interface GetNotificationDispatchResponse {
  id: string;
  referenceType: string | null;
  referenceId: string | null;
  channel: NotificationChannelType;
  templateId: string | null;
  payload: string | null;
  status: DispatchStatus;
  retryCount: number;
  nextRetryAt: string | null;
  lastError: string | null;
  dispatchedAt: string | null;
  createdAt: string;
}

/** GET /notification-dispatches/{dispatchId} */
export function getNotificationDispatch(
  dispatchId: string,
): Promise<GetNotificationDispatchResponse> {
  return apiClient
    .get(`${BASE_PATH}/notification-dispatches/${dispatchId}`)
    .then((res) => res.data);
}
