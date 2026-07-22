'use client';

import { useCallback, useState } from 'react';
import { useTranslations } from 'next-intl';
import {
  AppLoading,
  AppEmpty,
  AppModal,
  AppTabs,
  AppTabsList,
  AppTabsTrigger,
  AppTabsContent,
  CancelButton,
  DeleteButton,
  toast,
} from '@/shared/ui';
import { EntityDetailHeader, AuditTrailButton } from '@/shared/entity';
import {
  OrderSummaryCard,
  OrderItems,
  OrderCustomer,
  OrderTimeline,
  OrderActions,
  OrderMetadata,
  type OrderTimelineEvent,
} from '@/shared/commerce';
import { useOrderQuery, useCancelOrderMutation } from '../api/orders.queries';
import {
  useOrderRealtimeUpdates,
  type OrderStatusChangedEvent,
} from '../hooks/useOrderRealtimeUpdates';

export function OrderDetailPage({ orderId }: { orderId: string }) {
  const t = useTranslations('orders.detail');
  const tNav = useTranslations('nav');
  const { data: order, isLoading } = useOrderQuery(orderId);
  const cancelMutation = useCancelOrderMutation();
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [liveEvents, setLiveEvents] = useState<OrderTimelineEvent[]>([]);

  const handleRealtimeEvent = useCallback(
    (event: OrderStatusChangedEvent) => {
      if (event.orderId !== orderId) return;
      setLiveEvents((prev) => [
        ...prev,
        {
          status: event.status,
          timestamp: event.timestamp,
          operator: event.operator,
          message: event.message,
        },
      ]);
    },
    [orderId],
  );
  useOrderRealtimeUpdates(handleRealtimeEvent);

  if (isLoading) {
    return <AppLoading />;
  }

  if (!order) {
    return <AppEmpty title={t('notFound')} />;
  }

  async function handleCancel() {
    try {
      await cancelMutation.mutateAsync(orderId);
      toast.success(t('cancelSuccess'));
      setConfirmOpen(false);
    } catch {
      // Order may already be cancelled/completed — the Order service returns
      // 400 for that (see docs/backend/order/README.md); shown as a normal error.
      toast.error(t('cancelError'));
    }
  }

  return (
    <div className="space-y-4">
      <EntityDetailHeader
        breadcrumbItems={[
          { label: tNav('dashboard'), href: '/' },
          { label: tNav('items.orders'), href: '/orders' },
          { label: order.id },
        ]}
        title={t('title', { id: order.id })}
        actions={
          <>
            <OrderActions onCancel={() => setConfirmOpen(true)} />
            <AuditTrailButton service="Order" entityId={order.id} />
          </>
        }
      />

      <OrderSummaryCard order={order} />

      <AppTabs defaultValue="products">
        <AppTabsList>
          <AppTabsTrigger value="products">{t('tabs.products')}</AppTabsTrigger>
          <AppTabsTrigger value="customer">{t('tabs.customer')}</AppTabsTrigger>
          <AppTabsTrigger value="timeline">{t('tabs.timeline')}</AppTabsTrigger>
          <AppTabsTrigger value="metadata">{t('tabs.metadata')}</AppTabsTrigger>
        </AppTabsList>

        <AppTabsContent value="products">
          <OrderItems items={order.items ?? []} />
        </AppTabsContent>

        <AppTabsContent value="customer">
          <OrderCustomer customerId={order.customerId} />
        </AppTabsContent>

        <AppTabsContent value="timeline">
          <OrderTimeline currentStatus={order.status} events={liveEvents} />
        </AppTabsContent>

        <AppTabsContent value="metadata">
          <OrderMetadata order={order} />
        </AppTabsContent>
      </AppTabs>

      <AppModal
        open={confirmOpen}
        onOpenChange={setConfirmOpen}
        title={t('cancelConfirmTitle')}
        description={t('cancelConfirmDescription')}
        footer={
          <>
            <CancelButton onClick={() => setConfirmOpen(false)}>
              {t('cancelConfirmDismiss')}
            </CancelButton>
            <DeleteButton onClick={handleCancel} loading={cancelMutation.isPending}>
              {t('cancelConfirmConfirm')}
            </DeleteButton>
          </>
        }
      />
    </div>
  );
}
