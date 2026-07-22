'use client';

import { useTranslations } from 'next-intl';
import { AppSelect } from '@/shared/ui';
import { WarehouseSelector, type WarehouseOption } from './WarehouseSelector';

export interface InventoryFiltersValue {
  warehouseId?: string;
  type?: string;
}

export interface InventoryFiltersProps {
  value: InventoryFiltersValue;
  onChange: (next: InventoryFiltersValue) => void;
  warehouses: WarehouseOption[];
  /** Omit (or pass an empty array) to hide the type filter — e.g. it doesn't apply to a plain inventory-record list. */
  typeOptions?: { value: string; label: string }[];
}

/** Filter content for Stock / Stock Transactions — warehouse + (optionally) transaction type. */
export function InventoryFilters({
  value,
  onChange,
  warehouses,
  typeOptions,
}: InventoryFiltersProps) {
  const t = useTranslations('inventoryUi.filters');

  return (
    <div className="space-y-3">
      <div className="space-y-1.5">
        <label className="text-sm font-medium">{t('warehouse')}</label>
        <WarehouseSelector
          warehouses={warehouses}
          value={value.warehouseId}
          onChange={(warehouseId) => onChange({ ...value, warehouseId })}
        />
      </div>
      {typeOptions && typeOptions.length > 0 ? (
        <div className="space-y-1.5">
          <label className="text-sm font-medium">{t('type')}</label>
          <AppSelect
            options={typeOptions}
            value={value.type}
            onValueChange={(type) => onChange({ ...value, type })}
            placeholder={t('anyType')}
          />
        </div>
      ) : null}
    </div>
  );
}
