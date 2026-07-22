'use client';

import { useTranslations } from 'next-intl';
import { AppBadge } from '@/shared/ui';
import { cn } from '@/shared/lib/utils';

export interface StockQuantityProps {
  value: number;
  className?: string;
}

/**
 * "In stock"/"Out of stock" is derived from the real `quantity` field
 * (quantity > 0), not an invented backend status — GetInventoryResponse has
 * no status field at all. See docs/modules/inventory-management.md.
 */
export function StockQuantity({ value, className }: StockQuantityProps) {
  const t = useTranslations('inventoryUi.stockQuantity');
  return (
    <span className={cn('inline-flex items-center gap-2', className)}>
      <span className="font-medium tabular-nums">{value}</span>
      <AppBadge variant={value > 0 ? 'default' : 'outline'}>
        {value > 0 ? t('inStock') : t('outOfStock')}
      </AppBadge>
    </span>
  );
}
