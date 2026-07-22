'use client';

import { useTranslations } from 'next-intl';
import { AppSelect } from '@/shared/ui';

export interface WarehouseOption {
  id: string;
  label: string;
}

export interface WarehouseSelectorProps {
  warehouses: WarehouseOption[];
  value?: string;
  onChange: (warehouseId: string) => void;
}

/**
 * Prop-driven, not self-fetching — `warehouses` comes from
 * features/inventory's `useLocalWarehousesQuery` (the set known to this
 * browser, since there's no list endpoint). Keeps this layer purely
 * presentational, same as shared/commerce.
 */
export function WarehouseSelector({ warehouses, value, onChange }: WarehouseSelectorProps) {
  const t = useTranslations('inventoryUi.warehouseSelector');
  const options = warehouses.map((w) => ({ value: w.id, label: w.label }));
  return (
    <AppSelect
      options={options}
      value={value}
      onValueChange={onChange}
      placeholder={t('placeholder')}
    />
  );
}
