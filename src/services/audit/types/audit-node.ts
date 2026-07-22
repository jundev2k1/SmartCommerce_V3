import type { AuditAction } from './audit-action';

export interface AuditFieldChange {
  propertyName: string | null;
  oldValue: string | null;
  newValue: string | null;
}

export interface AuditNode {
  nodeId: string;
  parentNodeId: string | null;
  depth: number;
  entityType: string | null;
  entityId: string | null;
  action: AuditAction;
  changes: AuditFieldChange[] | null;
  children: AuditNode[] | null;
}

export interface AuditMetadata {
  actor: string | null;
  service: string | null;
  clientIp: string | null;
  userAgent: string | null;
  businessAction: string | null;
  reason: string | null;
  requestPath: string | null;
  traceId: string | null;
}
