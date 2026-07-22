'use client';

import { useTranslations } from 'next-intl';
import { AppSelect } from '@/shared/ui';
import type { ProductVariationResponse } from '@/services/product';

export interface VariantSelectorProps {
  variations: ProductVariationResponse[];
  value?: string;
  onChange: (variationId: string) => void;
}

/**
 * Hides variations whose free-text `status` is "Inactive"/"Discontinued" —
 * a presentation choice for a customer-facing picker, not an invented backend
 * rule. Variations with a null status are treated as available. See
 * docs/backend/product/README.md for the status field itself.
 */
export function VariantSelector({ variations, value, onChange }: VariantSelectorProps) {
  const t = useTranslations('commerce.variantSelector');
  const available = variations.filter(
    (v) => v.status !== 'Inactive' && v.status !== 'Discontinued',
  );

  if (available.length === 0) {
    return <p className="text-muted-foreground text-sm">{t('unavailable')}</p>;
  }

  const options = available.map((v) => ({
    value: v.id,
    label: `${v.sku ?? v.id} — ${v.price.toFixed(2)}`,
  }));

  return (
    <AppSelect
      options={options}
      value={value}
      onValueChange={onChange}
      placeholder={t('placeholder')}
    />
  );
}
