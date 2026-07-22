'use client';

import { useTranslations } from 'next-intl';
import { AppCard, AppCardHeader, AppCardTitle, AppCardContent } from '@/shared/ui';
import { StockQuantity } from './StockQuantity';

export interface InventorySummaryCardProps {
  productName: string;
  variationLabel?: string;
  warehouseName?: string;
  quantity: number;
}

/**
 * Read-only summary of one inventory record — deliberately does not
 * re-display full Catalog data (name/sku only, no description/images/etc.),
 * per the brief's "do not duplicate Product information already available in
 * Catalog."
 */
export function InventorySummaryCard({
  productName,
  variationLabel,
  warehouseName,
  quantity,
}: InventorySummaryCardProps) {
  const t = useTranslations('inventoryUi.summaryCard');
  return (
    <AppCard>
      <AppCardHeader>
        <AppCardTitle>{productName}</AppCardTitle>
      </AppCardHeader>
      <AppCardContent className="space-y-2 text-sm">
        {variationLabel ? (
          <div className="flex justify-between">
            <span className="text-muted-foreground">{t('variant')}</span>
            <span>{variationLabel}</span>
          </div>
        ) : null}
        {warehouseName ? (
          <div className="flex justify-between">
            <span className="text-muted-foreground">{t('warehouse')}</span>
            <span>{warehouseName}</span>
          </div>
        ) : null}
        <div className="flex justify-between">
          <span className="text-muted-foreground">{t('quantity')}</span>
          <StockQuantity value={quantity} />
        </div>
      </AppCardContent>
    </AppCard>
  );
}
