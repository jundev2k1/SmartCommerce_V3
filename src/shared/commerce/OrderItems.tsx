'use client';

import { useTranslations } from 'next-intl';
import {
  AppTable,
  AppTableHeader,
  AppTableBody,
  AppTableRow,
  AppTableHead,
  AppTableCell,
} from '@/shared/ui';
import { ProductPrice } from './ProductPrice';
import type { GetOrderItemResponse } from '@/services/order';

export interface OrderItemsProps {
  items: GetOrderItemResponse[];
}

/**
 * Line-item table extracted from OrderDetailPage so any future order-related
 * screen (e.g. a printable order summary) can reuse it. Only shows what
 * `GetOrderResponse.items` actually has — no variant/discount columns, since
 * neither exists on this DTO (see docs/modules/order-management.md).
 */
export function OrderItems({ items }: OrderItemsProps) {
  const t = useTranslations('commerce.orderItems');

  return (
    <div className="rounded-md border">
      <AppTable>
        <AppTableHeader>
          <AppTableRow>
            <AppTableHead>{t('product')}</AppTableHead>
            <AppTableHead>{t('unitPrice')}</AppTableHead>
            <AppTableHead>{t('quantity')}</AppTableHead>
            <AppTableHead>{t('lineTotal')}</AppTableHead>
          </AppTableRow>
        </AppTableHeader>
        <AppTableBody>
          {items.map((item, index) => (
            <AppTableRow key={`${item.productId}-${index}`}>
              <AppTableCell>{item.productName ?? item.productId}</AppTableCell>
              <AppTableCell>
                <ProductPrice value={item.unitPrice} />
              </AppTableCell>
              <AppTableCell>{item.quantity}</AppTableCell>
              <AppTableCell>
                <ProductPrice value={item.lineTotal} />
              </AppTableCell>
            </AppTableRow>
          ))}
        </AppTableBody>
      </AppTable>
    </div>
  );
}
