'use client';

import { useTranslations } from 'next-intl';
import { EntityMetadata } from '@/shared/entity';
import type { GetOrderResponse } from '@/services/order';

export interface OrderMetadataProps {
  order: GetOrderResponse;
}

/** Thin Order-specific wrapper over shared/entity's generic EntityMetadata. */
export function OrderMetadata({ order }: OrderMetadataProps) {
  const t = useTranslations('commerce.orderMetadata');
  return (
    <EntityMetadata
      items={[
        { label: t('orderId'), value: order.id },
        { label: t('customerId'), value: order.customerId },
        { label: t('createdAt'), value: new Date(order.createdAt).toLocaleString() },
        { label: t('updatedAt'), value: new Date(order.updatedAt).toLocaleString() },
      ]}
    />
  );
}
