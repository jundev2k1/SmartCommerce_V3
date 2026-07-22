import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { AuditNode, AuditMetadata } from './types/audit-node';

export interface GetAuditLogResponse {
  id: string;
  rootEntityType: string | null;
  rootEntityId: string | null;
  service: string | null;
  correlationId: string | null;
  root: AuditNode;
  metadata: AuditMetadata;
  timestamp: string;
  receivedAt: string;
}

/** GET /audit-logs/{auditLogId} — admin only. */
export function getAuditLog(auditLogId: string): Promise<GetAuditLogResponse> {
  return apiClient.get(`${BASE_PATH}/audit-logs/${auditLogId}`).then((res) => res.data);
}
