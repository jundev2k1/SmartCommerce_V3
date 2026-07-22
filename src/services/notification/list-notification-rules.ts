import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { BackendPaginatedResult } from '@/services/shared/paginated-result';
import type { NotificationRuleStatus } from './types/enums';

export interface ListNotificationRulesParams {
  eventType?: string;
  page?: number;
  pageSize?: number;
}

export interface NotificationRuleSummaryResponse {
  id: string;
  name: string | null;
  eventType: string | null;
  status: NotificationRuleStatus;
  targetCount: number;
  createdAt: string;
}

/** GET /notification-rules */
export function listNotificationRules(
  params: ListNotificationRulesParams = {},
): Promise<BackendPaginatedResult<NotificationRuleSummaryResponse>> {
  return apiClient.get(`${BASE_PATH}/notification-rules`, { params }).then((res) => res.data);
}
