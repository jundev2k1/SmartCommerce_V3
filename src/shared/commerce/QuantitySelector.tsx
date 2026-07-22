'use client';

import { Minus, Plus } from 'lucide-react';
import { useTranslations } from 'next-intl';
import { IconButton } from '@/shared/ui';

export interface QuantitySelectorProps {
  value: number;
  onChange: (value: number) => void;
  min?: number;
  max?: number;
}

export function QuantitySelector({ value, onChange, min = 1, max }: QuantitySelectorProps) {
  const t = useTranslations('commerce.quantitySelector');
  return (
    <div className="flex items-center gap-2">
      <IconButton
        aria-label={t('decrease')}
        onClick={() => onChange(Math.max(min, value - 1))}
        disabled={value <= min}
      >
        <Minus />
      </IconButton>
      <span className="w-8 text-center text-sm tabular-nums">{value}</span>
      <IconButton
        aria-label={t('increase')}
        onClick={() => onChange(max != null ? Math.min(max, value + 1) : value + 1)}
        disabled={max != null && value >= max}
      >
        <Plus />
      </IconButton>
    </div>
  );
}
