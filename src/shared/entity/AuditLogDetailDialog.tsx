'use client';

import { useQuery } from '@tanstack/react-query';
import { useTranslations } from 'next-intl';
import { getAuditLog, type AuditNode } from '@/services/audit';
import {
  AppDialog,
  AppDialogContent,
  AppDialogHeader,
  AppDialogTitle,
  AppDialogDescription,
  AppLoading,
  AppEmpty,
  AppTable,
  AppTableHeader,
  AppTableBody,
  AppTableRow,
  AppTableHead,
  AppTableCell,
} from '@/shared/ui';
import { EntityMetadata } from './EntityMetadata';
import { EntityStatusBadge } from './EntityStatusBadge';

export interface AuditLogDetailDialogProps {
  auditLogId: string | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

type Translator = ReturnType<typeof useTranslations>;

/** One entity's before/after change set, recursing into `children` for multi-entity transactions (e.g. Order + OrderItems). */
function AuditChangeNode({ node, t }: { node: AuditNode; t: Translator }) {
  const changes = node.changes ?? [];
  const children = node.children ?? [];

  return (
    <div className="space-y-2 border-l-2 pl-3">
      <div className="flex flex-wrap items-center gap-2">
        <span className="text-sm font-medium">{node.entityType ?? t('unknownEntity')}</span>
        {node.entityId ? (
          <span className="text-muted-foreground font-mono text-xs">{node.entityId}</span>
        ) : null}
        <span className="text-muted-foreground flex items-center gap-1 text-xs">
          {t('changes.action')}
          <EntityStatusBadge status={node.action} />
        </span>
      </div>
      {changes.length > 0 ? (
        <AppTable>
          <AppTableHeader>
            <AppTableRow>
              <AppTableHead>{t('changes.field')}</AppTableHead>
              <AppTableHead>{t('changes.oldValue')}</AppTableHead>
              <AppTableHead>{t('changes.newValue')}</AppTableHead>
            </AppTableRow>
          </AppTableHeader>
          <AppTableBody>
            {changes.map((change, index) => (
              <AppTableRow key={`${change.propertyName ?? 'field'}-${index}`}>
                <AppTableCell className="font-medium">{change.propertyName ?? '—'}</AppTableCell>
                <AppTableCell className="text-muted-foreground break-all">
                  {change.oldValue ?? '—'}
                </AppTableCell>
                <AppTableCell className="break-all">{change.newValue ?? '—'}</AppTableCell>
              </AppTableRow>
            ))}
          </AppTableBody>
        </AppTable>
      ) : (
        <p className="text-muted-foreground text-xs">{t('changes.none')}</p>
      )}
      {children.map((child) => (
        <AuditChangeNode key={child.nodeId} node={child} t={t} />
      ))}
    </div>
  );
}

/**
 * Full detail for a single audit entry — fetched on demand from
 * GET /audit-logs/{id} (getAuditLog) when opened, rather than embedded in
 * the list response (ListAuditLogs only returns summaries, see
 * docs/backend/audit/README.md).
 */
export function AuditLogDetailDialog({
  auditLogId,
  open,
  onOpenChange,
}: AuditLogDetailDialogProps) {
  const t = useTranslations('entity.auditDetail');
  const { data, isLoading } = useQuery({
    queryKey: ['audit-log', auditLogId],
    queryFn: () => getAuditLog(auditLogId as string),
    enabled: open && auditLogId !== null,
  });

  return (
    <AppDialog open={open} onOpenChange={onOpenChange}>
      <AppDialogContent className="max-h-[85vh] overflow-y-auto sm:max-w-2xl">
        <AppDialogHeader>
          <AppDialogTitle>{t('title')}</AppDialogTitle>
          <AppDialogDescription>{t('description')}</AppDialogDescription>
        </AppDialogHeader>
        {isLoading ? (
          <AppLoading />
        ) : !data ? (
          <AppEmpty description={t('notFound')} />
        ) : (
          <div className="space-y-4">
            <EntityMetadata
              items={[
                { label: t('metadata.actor'), value: data.metadata.actor ?? '—' },
                { label: t('metadata.businessAction'), value: data.metadata.businessAction ?? '—' },
                { label: t('metadata.reason'), value: data.metadata.reason ?? '—' },
                { label: t('metadata.service'), value: data.service ?? '—' },
                {
                  label: t('metadata.timestamp'),
                  value: new Date(data.timestamp).toLocaleString(),
                },
                { label: t('metadata.correlationId'), value: data.correlationId ?? '—' },
                { label: t('metadata.clientIp'), value: data.metadata.clientIp ?? '—' },
                { label: t('metadata.requestPath'), value: data.metadata.requestPath ?? '—' },
                { label: t('metadata.traceId'), value: data.metadata.traceId ?? '—' },
                { label: t('metadata.userAgent'), value: data.metadata.userAgent ?? '—' },
              ]}
            />
            <div className="space-y-2">
              <p className="text-sm font-medium">{t('changes.title')}</p>
              <AuditChangeNode node={data.root} t={t} />
            </div>
          </div>
        )}
      </AppDialogContent>
    </AppDialog>
  );
}
