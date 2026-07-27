'use client';

import { useMemo } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { AppEmpty, AppLoading, PrimaryButton } from '@/shared/ui';
import { EntityHeader } from '@/shared/entity';
import { OrderSummaryCard } from '@/shared/commerce';
import { useLocalOrdersQuery } from '../api/orders.queries';
import { useOrderRealtimeUpdates } from '../hooks/useOrderRealtimeUpdates';

/**
 * Customer-facing order history — browser-scoped local-orders data, unlike
 * the admin Orders list (OrdersListPage), which now uses the real
 * `SearchOrders` endpoint. That endpoint is Admin-only and there's no
 * customer-scoped "list my orders" endpoint (see
 * docs/modules/order-management.md), so this page has no real list to call
 * and stays on the local-orders/GetOrder approach; presented as a simple
 * card grid instead of an admin data table.
 */
export function OrderHistoryListPage() {
  const t = useTranslations('orders.history');
  const router = useRouter();
  const { orders, isLoading, hasAny } = useLocalOrdersQuery();
  useOrderRealtimeUpdates();

  const sorted = useMemo(
    () =>
      [...orders].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()),
    [orders],
  );

  if (!hasAny) {
    return (
      <div className="space-y-4">
        <EntityHeader title={t('title')} />
        <AppEmpty
          title={t('emptyTitle')}
          description={t('emptyDescription')}
          action={
            <PrimaryButton onClick={() => router.push('/shop')}>
              {t('browseProducts')}
            </PrimaryButton>
          }
        />
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <EntityHeader title={t('title')} />
      {isLoading ? (
        <AppLoading />
      ) : (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {sorted.map((order) => (
            <Link key={order.id} href={`/orders/${order.id}`} className="block">
              <OrderSummaryCard order={order} />
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
