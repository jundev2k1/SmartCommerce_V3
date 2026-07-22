'use client';

import { useTranslations } from 'next-intl';
import { AppEmpty } from '@/shared/ui';
import { StockStatusBadge } from './StockStatusBadge';
import type { InventoryTransactionDto } from '@/services/inventory';

export interface TransactionTimelineProps {
  transactions: InventoryTransactionDto[];
}

/**
 * Unlike Order's OrderTimeline (a literal placeholder — no history data
 * exists on that contract), GetInventoryHistory genuinely returns a
 * transaction list, so this renders real data. "Quantity before" is
 * deliberately not shown/derived: `quantity` and `quantityAfter` don't have a
 * confirmed sign/delta convention across StockIn/StockOut/Adjust (Adjust's
 * request is an absolute `newQuantity`, not a delta), so computing a "before"
 * value would be guessing. See docs/backend/inventory/README.md and
 * docs/modules/inventory-management.md.
 */
export function TransactionTimeline({ transactions }: TransactionTimelineProps) {
  const t = useTranslations('inventoryUi.transactionTimeline');

  if (transactions.length === 0) {
    return <AppEmpty description={t('empty')} />;
  }

  return (
    <ol className="space-y-4 border-l pl-4">
      {transactions.map((tx) => (
        <li key={tx.id} className="space-y-1">
          <div className="flex items-center gap-2">
            <StockStatusBadge value={tx.type} />
            <span className="text-muted-foreground text-sm">
              {new Date(tx.createdAt).toLocaleString()}
            </span>
          </div>
          <p className="text-sm">
            {t('quantityChanged')}: {tx.quantity} · {t('quantityAfter')}: {tx.quantityAfter}
          </p>
          {tx.reason ? <p className="text-muted-foreground text-sm">{tx.reason}</p> : null}
        </li>
      ))}
    </ol>
  );
}
