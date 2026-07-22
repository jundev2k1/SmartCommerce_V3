'use client';

import { useTranslations } from 'next-intl';
import { AppBadge, type AppBadgeProps } from '@/shared/ui';
import { OrderStatus } from '@/services/order';

export interface OrderStatusBadgeProps {
  status: OrderStatus | null | undefined;
}

const VARIANT_BY_STATUS: Record<OrderStatus, AppBadgeProps['variant']> = {
  [OrderStatus.Pending]: 'outline',
  [OrderStatus.Confirmed]: 'secondary',
  [OrderStatus.Completed]: 'default',
  [OrderStatus.Cancelled]: 'destructive',
};

/** Also used by OrdersListPage's status filter, so the dropdown labels match this badge. */
export const ORDER_STATUS_LABEL_KEYS: Record<OrderStatus, string> = {
  [OrderStatus.Pending]: 'pending',
  [OrderStatus.Confirmed]: 'confirmed',
  [OrderStatus.Completed]: 'completed',
  [OrderStatus.Cancelled]: 'cancelled',
};

/** Order.Domain/Enums/OrderStatus.cs — Pending=1, Confirmed=2, Cancelled=3, Completed=4. */
export function OrderStatusBadge({ status }: OrderStatusBadgeProps) {
  const t = useTranslations('commerce.orderStatus');
  if (status == null || !(status in ORDER_STATUS_LABEL_KEYS)) {
    return <AppBadge variant="outline">{t('unknown', { value: status ?? '—' })}</AppBadge>;
  }
  return (
    <AppBadge variant={VARIANT_BY_STATUS[status]}>{t(ORDER_STATUS_LABEL_KEYS[status])}</AppBadge>
  );
}
