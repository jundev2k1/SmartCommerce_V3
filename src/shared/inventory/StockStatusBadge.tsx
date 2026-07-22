'use client';

import { useTranslations } from 'next-intl';
import { AppBadge } from '@/shared/ui';

export interface StockStatusBadgeProps {
  value: number | null | undefined;
}

/**
 * Renders an opaque numeric code as a badge — reused for both `WarehouseStatus`
 * (1–2) and `InventoryTransactionType` (1–3), neither of which has a published
 * name mapping (see docs/backend/inventory/README.md). Always the same neutral
 * variant deliberately — "avoid hardcoded colors" per the Phase 5 brief, which
 * also happens to be the only honest choice when the values' meanings aren't
 * confirmed.
 */
export function StockStatusBadge({ value }: StockStatusBadgeProps) {
  const t = useTranslations('inventoryUi.stockStatusBadge');
  if (value == null) {
    return <AppBadge variant="outline">—</AppBadge>;
  }
  return <AppBadge variant="secondary">{t('code', { value })}</AppBadge>;
}
