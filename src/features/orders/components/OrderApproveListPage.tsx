'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { useQueryClient } from '@tanstack/react-query';
import type { ColumnDef } from '@tanstack/react-table';
import {
  AppDataTable,
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
import { OrderStatus, type SearchOrdersItemResponse } from '@/services/order';
import {
  useOrdersSearchQuery,
  useCancelOrderMutation,
  useCompleteOrderMutation,
  orderKeys,
} from '../api/orders.queries';
import { useOrderRealtimeUpdates } from '../hooks/useOrderRealtimeUpdates';

const PAGE_SIZE = 10;

/**
 * Confirmed orders awaiting fulfillment — server-side search via the real
 * Admin-only `SearchOrders` endpoint, filtered to `status = Confirmed`.
 * "Approve" calls the real `CompleteOrder` endpoint; once an order leaves
 * Confirmed, `useOrderRealtimeUpdates` invalidates the search query and it
 * naturally drops out of this list.
 */
export function OrderApproveListPage() {
  const t = useTranslations('orders.approve');
  const tCommon = useTranslations('common.actions');
  const router = useRouter();
  const queryClient = useQueryClient();
  useOrderRealtimeUpdates();

  const completeMutation = useCompleteOrderMutation();
  const cancelMutation = useCancelOrderMutation();
  const [cancelTarget, setCancelTarget] = useState<string | null>(null);
  const [page, setPage] = useState(1);

  const { data, isLoading } = useOrdersSearchQuery({
    status: OrderStatus.Confirmed,
    sortBy: 'createdAt',
    sortDescending: true,
    page,
    pageSize: PAGE_SIZE,
  });

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
    queryClient.invalidateQueries({ queryKey: orderKeys.searches() });
  }

  const columns: ColumnDef<SearchOrdersItemResponse>[] = [
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

  return (
    <div className="space-y-4">
      <EntityHeader
        title={t('title')}
        actions={<RefreshButton aria-label={t('refresh')} onClick={handleRefresh} />}
      />

      <AppDataTable<SearchOrdersItemResponse>
        columns={columns}
        data={data?.items ?? []}
        page={page}
        pageCount={data?.totalPages ?? 1}
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
