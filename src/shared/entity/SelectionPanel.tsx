'use client';

import type { ReactNode } from 'react';
import { useTranslations } from 'next-intl';
import { CancelButton, AppTooltip, DeleteButton } from '@/shared/ui';

export interface SelectionPanelProps {
  count: number;
  onClear: () => void;
  /** Extra bulk-action buttons, if the backend ever supports any — currently always disabled/TODO, see docs/decisions/0014. */
  bulkActions?: ReactNode;
}

/**
 * "N selected" bar shown above a table when AppDataTable's row selection is
 * non-empty. No backend service in this project exposes a bulk endpoint yet
 * (see docs/backend/coverage-report.md), so the default bulk action rendered
 * here is a disabled placeholder, not a real operation.
 */
export function SelectionPanel({ count, onClear, bulkActions }: SelectionPanelProps) {
  const t = useTranslations('entity.selection');

  if (count === 0) {
    return null;
  }

  return (
    <div className="bg-muted/50 flex items-center gap-2 rounded-md border px-3 py-2 text-sm">
      <span className="font-medium">{t('selectedCount', { count })}</span>
      <CancelButton size="sm" onClick={onClear}>
        {t('clear')}
      </CancelButton>
      <div className="ml-auto flex items-center gap-2">
        {bulkActions ?? (
          <AppTooltip content={t('bulkActionsUnavailable')}>
            <span>
              <DeleteButton size="sm" disabled>
                {t('bulkDelete')}
              </DeleteButton>
            </span>
          </AppTooltip>
        )}
      </div>
    </div>
  );
}
