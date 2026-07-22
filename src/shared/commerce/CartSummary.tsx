'use client';

import type { ReactNode } from 'react';
import { useTranslations } from 'next-intl';
import { AppCard, AppCardHeader, AppCardTitle, AppCardContent } from '@/shared/ui';
import { ProductPrice } from './ProductPrice';
import type { CartItemResponse } from '@/services/order';

export interface CartSummaryProps {
  items: CartItemResponse[];
  /** e.g. a "Proceed to checkout" button — rendered by the caller so this stays route-agnostic. */
  action?: ReactNode;
}

/** Totals-only sidebar box for the Cart page. See CheckoutSummary for the full line-item recap. */
export function CartSummary({ items, action }: CartSummaryProps) {
  const t = useTranslations('commerce.cartSummary');
  const itemCount = items.reduce((sum, item) => sum + item.quantity, 0);
  const subtotal = items.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0);

  return (
    <AppCard>
      <AppCardHeader>
        <AppCardTitle>{t('title')}</AppCardTitle>
      </AppCardHeader>
      <AppCardContent className="space-y-3">
        <div className="text-muted-foreground flex justify-between text-sm">
          <span>{t('items')}</span>
          <span>{itemCount}</span>
        </div>
        <div className="flex justify-between font-medium">
          <span>{t('subtotal')}</span>
          <ProductPrice value={subtotal} />
        </div>
        {action}
      </AppCardContent>
    </AppCard>
  );
}
