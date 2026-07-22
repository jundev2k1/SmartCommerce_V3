'use client';

import { useTranslations } from 'next-intl';
import { AppCard, AppCardHeader, AppCardTitle, AppCardContent } from '@/shared/ui';
import { OrderStatusBadge } from './OrderStatusBadge';
import { ProductPrice } from './ProductPrice';
import type { GetOrderResponse } from '@/services/order';

export interface OrderSummaryCardProps {
  order: GetOrderResponse;
}

/** Order-level at-a-glance card — id, status, total, created date. */
export function OrderSummaryCard({ order }: OrderSummaryCardProps) {
  const t = useTranslations('commerce.orderSummaryCard');
  return (
    <AppCard>
      <AppCardHeader>
        <AppCardTitle>{t('title', { id: order.id })}</AppCardTitle>
      </AppCardHeader>
      <AppCardContent className="space-y-2 text-sm">
        <div className="flex justify-between">
          <span className="text-muted-foreground">{t('status')}</span>
          <OrderStatusBadge status={order.status} />
        </div>
        <div className="flex justify-between">
          <span className="text-muted-foreground">{t('total')}</span>
          <ProductPrice value={order.totalAmount} />
        </div>
        <div className="flex justify-between">
          <span className="text-muted-foreground">{t('createdAt')}</span>
          <span>{new Date(order.createdAt).toLocaleString()}</span>
        </div>
      </AppCardContent>
    </AppCard>
  );
}
