'use client';

import type { ReactNode } from 'react';
import { ListFilter } from 'lucide-react';
import { useTranslations } from 'next-intl';
import {
  AppPopover,
  AppPopoverTrigger,
  AppPopoverContent,
  AppButton,
  IconButton,
} from '@/shared/ui';

export interface FilterPanelProps {
  children: ReactNode;
  /** Shown as a small dot on the trigger when at least one filter is active. */
  active?: boolean;
  /** Resets all filter state; the clear button only renders when this is provided and `active` is true. */
  onClear?: () => void;
}

/** Generic Popover wrapper around arbitrary filter form content — reused by every entity list, not Product-specific. */
export function FilterPanel({ children, active, onClear }: FilterPanelProps) {
  const t = useTranslations('entity.filters');

  return (
    <AppPopover>
      <AppPopoverTrigger asChild>
        <IconButton aria-label={t('title')} className="relative">
          <ListFilter />
          {active ? (
            <span className="bg-primary absolute top-1 right-1 size-2 rounded-full" />
          ) : null}
        </IconButton>
      </AppPopoverTrigger>
      <AppPopoverContent align="start" className="w-72 max-w-[calc(100vw-2rem)] space-y-3">
        {children}
        {active && onClear ? (
          <AppButton variant="ghost" size="sm" className="w-full" onClick={onClear}>
            {t('reset')}
          </AppButton>
        ) : null}
      </AppPopoverContent>
    </AppPopover>
  );
}
