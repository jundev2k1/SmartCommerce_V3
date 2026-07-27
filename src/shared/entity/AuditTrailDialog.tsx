'use client';

import { listAuditLogs, type AuditLogSummaryResponse } from '@/services/audit';
import {
  AppDrawer,
  AppDrawerContent,
  AppDrawerDescription,
  AppDrawerHeader,
  AppDrawerTitle,
  AppEmpty,
  AppLoading,
  AppTable,
  AppTableBody,
  AppTableCell,
  AppTableHead,
  AppTableHeader,
  AppTableRow,
  IconButton,
} from '@/shared/ui';
import { useQuery } from '@tanstack/react-query';
import { Eye } from 'lucide-react';
import { useTranslations } from 'next-intl';
import { useState } from 'react';
import { AuditLogDetailDialog } from './AuditLogDetailDialog';

export interface AuditTrailDialogProps {
  /** Backend "service" name filter (e.g. "Product", "User") — see docs/backend/audit/README.md. */
  service: string;
  entityId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

const PAGE_SIZE = 50;
// ListAuditLogs has no server-side entity-id filter (docs/backend/audit/README.md),
// so a target entity can sit past page 1 once a service has >50 logged events.
// Bounded to avoid unbounded sequential fetches for an entity with no history at all.
const MAX_PAGES_SEARCHED = 5;

interface EntityAuditResult {
  entries: AuditLogSummaryResponse[];
  /** True if we hit MAX_PAGES_SEARCHED without reaching the end of the service's log. */
  truncated: boolean;
}

async function fetchEntityAuditEntries(
  service: string,
  entityId: string,
): Promise<EntityAuditResult> {
  const entries: AuditLogSummaryResponse[] = [];
  for (let page = 1; page <= MAX_PAGES_SEARCHED; page++) {
    const result = await listAuditLogs({ service, page, pageSize: PAGE_SIZE });
    entries.push(...result.items.filter((entry) => entry.rootEntityId === entityId));
    if (!result.hasNextPage) return { entries, truncated: false };
  }
  return { entries, truncated: true };
}

/**
 * Generic audit-trail viewer — reused by every entity type (Users, Products,
 * Categories, Tags, ...). Lists summary entries for the entity (ListAuditLogs
 * has no server-side entity-id filter, so this walks pages for `service`,
 * bounded by MAX_PAGES_SEARCHED, and filters client-side by rootEntityId —
 * see docs/backend/audit/README.md). Each row's "view detail" action fetches
 * the full before/after change tree on demand via AuditLogDetailDialog
 * (GET /audit-logs/{id}), which the list endpoint doesn't include.
 */
export function AuditTrailDialog({ service, entityId, open, onOpenChange }: AuditTrailDialogProps) {
  const t = useTranslations('entity.auditTrail');
  const { data, isLoading } = useQuery({
    queryKey: ['audit', service, entityId],
    queryFn: () => fetchEntityAuditEntries(service, entityId),
    enabled: open,
  });
  const [detailId, setDetailId] = useState<string | null>(null);

  const entries = data?.entries ?? [];

  return (
    <>
      <AppDrawer
        open={open}
        onOpenChange={(next) => {
          onOpenChange(next);
          if (!next) setDetailId(null);
        }}
      >
        <AppDrawerContent className="data-[side=right]:sm:max-w-xl">
          <AppDrawerHeader>
            <AppDrawerTitle>{t('title')}</AppDrawerTitle>
            <AppDrawerDescription>{t('description')}</AppDrawerDescription>
          </AppDrawerHeader>
          <div className="flex-1 overflow-y-auto px-4 pb-4">
            {isLoading ? (
              <AppLoading />
            ) : entries.length === 0 ? (
              <AppEmpty description={data?.truncated ? t('emptyTruncated') : t('empty')} />
            ) : (
              <AppTable>
                <AppTableHeader>
                  <AppTableRow>
                    <AppTableHead>{t('columns.timestamp')}</AppTableHead>
                    <AppTableHead>{t('columns.entityType')}</AppTableHead>
                    <AppTableHead className="text-right">{t('columns.detail')}</AppTableHead>
                  </AppTableRow>
                </AppTableHeader>
                <AppTableBody>
                  {entries.map((entry) => (
                    <AppTableRow key={entry.id}>
                      <AppTableCell className="text-muted-foreground text-xs whitespace-nowrap">
                        {new Date(entry.timestamp).toLocaleString()}
                      </AppTableCell>
                      <AppTableCell className="text-sm font-medium">
                        {entry.rootEntityType ?? entry.service ?? '—'}
                      </AppTableCell>
                      <AppTableCell className="text-right">
                        <IconButton
                          aria-label={t('viewDetail')}
                          onClick={() => setDetailId(entry.id)}
                        >
                          <Eye />
                        </IconButton>
                      </AppTableCell>
                    </AppTableRow>
                  ))}
                </AppTableBody>
              </AppTable>
            )}
          </div>
        </AppDrawerContent>
      </AppDrawer>
      <AuditLogDetailDialog
        auditLogId={detailId}
        open={detailId !== null}
        onOpenChange={(next) => !next && setDetailId(null)}
      />
    </>
  );
}
