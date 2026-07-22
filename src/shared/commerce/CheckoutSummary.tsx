'use client';

import { useTranslations } from 'next-intl';
import { AppCard, AppCardHeader, AppCardTitle, AppCardContent } from '@/shared/ui';
import { ProductPrice } from './ProductPrice';
import type { CartItemResponse } from '@/services/order';

export interface CheckoutSummaryProps {
  items: CartItemResponse[];
}

/** Read-only line-item recap for the Checkout page — no quantity/remove controls, unlike CartSummary. */
export function CheckoutSummary({ items }: CheckoutSummaryProps) {
  const t = useTranslations('commerce.checkoutSummary');
  const total = items.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0);

  return (
    <AppCard>
      <AppCardHeader>
        <AppCardTitle>{t('title')}</AppCardTitle>
      </AppCardHeader>
      <AppCardContent className="space-y-3">
        {items.map((item) => (
          <div key={item.variationId} className="flex justify-between text-sm">
            <span>
              {item.productName} × {item.quantity}
            </span>
            <ProductPrice value={item.unitPrice * item.quantity} />
          </div>
        ))}
        <div className="flex justify-between border-t pt-3 font-medium">
          <span>{t('total')}</span>
          <ProductPrice value={total} />
        </div>
      </AppCardContent>
    </AppCard>
  );
}
