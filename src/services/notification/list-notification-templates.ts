import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { BackendPaginatedResult } from '@/services/shared/paginated-result';
import type { NotificationChannelType, NotificationTemplateStatus } from './types/enums';

export interface ListNotificationTemplatesParams {
  channel?: NotificationChannelType;
  page?: number;
  pageSize?: number;
}

export interface NotificationTemplateSummaryResponse {
  id: string;
  name: string | null;
  channel: NotificationChannelType;
  status: NotificationTemplateStatus;
  createdAt: string;
}

/** GET /notification-templates */
export function listNotificationTemplates(
  params: ListNotificationTemplatesParams = {},
): Promise<BackendPaginatedResult<NotificationTemplateSummaryResponse>> {
  return apiClient.get(`${BASE_PATH}/notification-templates`, { params }).then((res) => res.data);
}
