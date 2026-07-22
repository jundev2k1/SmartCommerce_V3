'use client';

import { useState, type ReactNode } from 'react';
import { useTranslations } from 'next-intl';
import { AppInput, SecondaryButton } from '@/shared/ui';
import { EntityToolbar } from '@/shared/entity';

export interface InventoryToolbarProps {
  onSearchChange?: (value: string) => void;
  searchPlaceholder?: string;
  filters?: ReactNode;
  /** Bakes in the recurring "open by id" need — no list endpoint means this
   *  is the only way to reach a record this browser hasn't already seen. */
  onOpenById: (id: string) => void;
  actions?: ReactNode;
}

export function InventoryToolbar({
  onSearchChange,
  searchPlaceholder,
  filters,
  onOpenById,
  actions,
}: InventoryToolbarProps) {
  const t = useTranslations('inventoryUi.toolbar');
  const [idValue, setIdValue] = useState('');

  function handleOpen() {
    const trimmed = idValue.trim();
    if (trimmed) {
      onOpenById(trimmed);
      setIdValue('');
    }
  }

  return (
    <EntityToolbar
      onSearchChange={onSearchChange}
      searchPlaceholder={searchPlaceholder}
      filters={filters}
      actions={
        <>
          <div className="flex items-center gap-1">
            <AppInput
              value={idValue}
              onChange={(e) => setIdValue(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleOpen()}
              placeholder={t('openByIdPlaceholder')}
              className="w-48"
            />
            <SecondaryButton onClick={handleOpen} disabled={!idValue.trim()}>
              {t('open')}
            </SecondaryButton>
          </div>
          {actions}
        </>
      }
    />
  );
}
