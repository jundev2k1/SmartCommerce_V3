'use client';

import { useQuery } from '@tanstack/react-query';
import { useTranslations } from 'next-intl';
import { listAuditLogs, type AuditLogSummaryResponse } from '@/services/audit';
import { AppModal, AppEmpty, AppLoading } from '@/shared/ui';

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
  for (let page = 1; page <= MAX_PAGES_SEARCHED; page++) {
    const result = await listAuditLogs({ service, page, pageSize: PAGE_SIZE });
    const entries = result.items.filter((entry) => entry.rootEntityId === entityId);
    if (entries.length > 0) return { entries, truncated: false };
    if (!result.hasNextPage) return { entries: [], truncated: false };
  }
  return { entries: [], truncated: true };
}

/**
 * Generic audit-trail viewer — reused by every entity type (Users, Products,
 * Categories, Tags, ...). ListAuditLogs has no server-side entity-id filter,
 * so this walks pages for `service` (bounded by MAX_PAGES_SEARCHED) and filters
 * client-side by rootEntityId. See docs/backend/audit/README.md.
 */
export function AuditTrailDialog({ service, entityId, open, onOpenChange }: AuditTrailDialogProps) {
  const t = useTranslations('entity.auditTrail');
  const { data, isLoading } = useQuery({
    queryKey: ['audit', service, entityId],
    queryFn: () => fetchEntityAuditEntries(service, entityId),
    enabled: open,
  });

  const entries = data?.entries ?? [];

  return (
    <AppModal
      open={open}
      onOpenChange={onOpenChange}
      title={t('title')}
      description={t('description')}
    >
      {isLoading ? (
        <AppLoading />
      ) : entries.length === 0 ? (
        <AppEmpty description={data?.truncated ? t('emptyTruncated') : t('empty')} />
      ) : (
        <ul className="max-h-80 space-y-2 overflow-y-auto">
          {entries.map((entry) => (
            <li key={entry.id} className="rounded-md border p-2 text-sm">
              <p className="font-medium">{entry.rootEntityType ?? entry.service ?? 'Unknown'}</p>
              <p className="text-muted-foreground">{new Date(entry.timestamp).toLocaleString()}</p>
            </li>
          ))}
        </ul>
      )}
    </AppModal>
  );
}
