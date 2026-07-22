'use client';

import { useMemo, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { useQueryClient } from '@tanstack/react-query';
import type { ColumnDef } from '@tanstack/react-table';
import {
  AppDataTable,
  AppEmpty,
  AppModal,
  CancelButton,
  DeleteButton,
  IconButton,
  RefreshButton,
  toast,
} from '@/shared/ui';
import { EntityHeader } from '@/shared/entity';
import { OrderStatusBadge, ProductPrice } from '@/shared/commerce';
import { Eye, CheckCircle2, XCircle } from 'lucide-react';
import { OrderStatus, type GetOrderResponse } from '@/services/order';
import {
  useLocalOrdersQuery,
  useCancelOrderMutation,
  useCompleteOrderMutation,
  orderKeys,
} from '../api/orders.queries';
import { useOrderRealtimeUpdates } from '../hooks/useOrderRealtimeUpdates';

const PAGE_SIZE = 10;

/**
 * Confirmed orders awaiting fulfillment — filtered client-side from the same
 * browser-scoped local-orders data every order view uses (no list endpoint
 * exists, see docs/modules/order-management.md). "Approve" calls the real
 * `CompleteOrder` endpoint (admin-only); once an order leaves Confirmed it
 * naturally drops out of this filtered view on the next render.
 */
export function OrderApproveListPage() {
  const t = useTranslations('orders.approve');
  const tCommon = useTranslations('common.actions');
  const router = useRouter();
  const queryClient = useQueryClient();
  const { orders, isLoading, hasAny } = useLocalOrdersQuery();
  useOrderRealtimeUpdates();

  const completeMutation = useCompleteOrderMutation();
  const cancelMutation = useCancelOrderMutation();
  const [cancelTarget, setCancelTarget] = useState<string | null>(null);
  const [page, setPage] = useState(1);

  const pending = useMemo(
    () => orders.filter((order) => order.status === OrderStatus.Confirmed),
    [orders],
  );
  const pageCount = Math.max(1, Math.ceil(pending.length / PAGE_SIZE));
  const pageItems = pending.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  async function handleApprove(orderId: string) {
    try {
      await completeMutation.mutateAsync(orderId);
      toast.success(t('approveSuccess'));
    } catch {
      toast.error(t('approveError'));
    }
  }

  async function handleCancel() {
    if (!cancelTarget) return;
    try {
      await cancelMutation.mutateAsync(cancelTarget);
      toast.success(t('cancelSuccess'));
      setCancelTarget(null);
    } catch {
      toast.error(t('cancelError'));
    }
  }

  function handleRefresh() {
    queryClient.invalidateQueries({ queryKey: orderKeys.details() });
  }

  const columns: ColumnDef<GetOrderResponse>[] = [
    { accessorKey: 'id', header: t('table.orderId') },
    { accessorKey: 'customerId', header: t('table.customerId') },
    {
      id: 'status',
      header: t('table.status'),
      cell: ({ row }) => <OrderStatusBadge status={row.original.status} />,
    },
    {
      id: 'total',
      header: t('table.total'),
      cell: ({ row }) => <ProductPrice value={row.original.totalAmount} />,
    },
    {
      id: 'createdAt',
      header: t('table.createdAt'),
      cell: ({ row }) => new Date(row.original.createdAt).toLocaleString(),
    },
  ];

  if (!hasAny) {
    return (
      <div className="space-y-4">
        <EntityHeader title={t('title')} />
        <AppEmpty title={t('emptyTitle')} description={t('emptyDescription')} />
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <EntityHeader
        title={t('title')}
        actions={<RefreshButton aria-label={t('refresh')} onClick={handleRefresh} />}
      />

      <AppDataTable<GetOrderResponse>
        columns={columns}
        data={pageItems}
        page={page}
        pageCount={pageCount}
        onPageChange={setPage}
        isLoading={isLoading}
        emptyTitle={t('emptyTitle')}
        emptyDescription={t('emptyDescription')}
        getRowId={(row) => row.id}
        rowActions={(order) => (
          <div className="flex gap-1">
            <IconButton
              aria-label={t('approveAction')}
              onClick={() => handleApprove(order.id)}
              disabled={completeMutation.isPending}
            >
              <CheckCircle2 />
            </IconButton>
            <IconButton aria-label={t('cancelAction')} onClick={() => setCancelTarget(order.id)}>
              <XCircle />
            </IconButton>
            <IconButton
              aria-label={tCommon('view')}
              onClick={() => router.push(`/orders/${order.id}`)}
            >
              <Eye />
            </IconButton>
          </div>
        )}
      />

      <AppModal
        open={cancelTarget !== null}
        onOpenChange={(open) => !open && setCancelTarget(null)}
        title={t('cancelConfirmTitle')}
        description={t('cancelConfirmDescription')}
        footer={
          <>
            <CancelButton onClick={() => setCancelTarget(null)}>
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
