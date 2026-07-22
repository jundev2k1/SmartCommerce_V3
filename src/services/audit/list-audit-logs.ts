import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { BackendPaginatedResult } from '@/services/shared/paginated-result';

export interface ListAuditLogsParams {
  service?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}

export interface AuditLogSummaryResponse {
  id: string;
  rootEntityType: string | null;
  rootEntityId: string | null;
  service: string | null;
  correlationId: string | null;
  timestamp: string;
}

/** GET /audit-logs — paginated, filterable list. Admin only. */
export function listAuditLogs(
  params: ListAuditLogsParams = {},
): Promise<BackendPaginatedResult<AuditLogSummaryResponse>> {
  return apiClient.get(`${BASE_PATH}/audit-logs`, { params }).then((res) => res.data);
}
