import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { BackendPaginatedResult } from '@/services/shared/paginated-result';
import type { CampaignExecutionType, CampaignStatus } from './types/enums';

export interface ListNotificationCampaignsParams {
  status?: CampaignStatus;
  page?: number;
  pageSize?: number;
}

export interface NotificationCampaignSummaryResponse {
  id: string;
  name: string | null;
  status: CampaignStatus;
  executionType: CampaignExecutionType;
  nextExecutionAt: string | null;
  createdAt: string;
}

/** GET /notification-campaigns */
export function listNotificationCampaigns(
  params: ListNotificationCampaignsParams = {},
): Promise<BackendPaginatedResult<NotificationCampaignSummaryResponse>> {
  return apiClient.get(`${BASE_PATH}/notification-campaigns`, { params }).then((res) => res.data);
}
