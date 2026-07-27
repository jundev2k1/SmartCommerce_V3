'use client';

import { useMemo, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { useQueryClient } from '@tanstack/react-query';
import type { ColumnDef, RowSelectionState } from '@tanstack/react-table';
import { AppDataTable, AppInput, AppSelect, IconButton, RefreshButton } from '@/shared/ui';
import { EntityHeader, EntityToolbar, FilterPanel, SelectionPanel } from '@/shared/entity';
import { OrderStatusBadge, ProductPrice, ORDER_STATUS_LABEL_KEYS } from '@/shared/commerce';
import { Eye } from 'lucide-react';
import { OrderStatus, type SearchOrdersItemResponse } from '@/services/order';
import { useOrdersSearchQuery, orderKeys } from '../api/orders.queries';
import { useOrderRealtimeUpdates } from '../hooks/useOrderRealtimeUpdates';

const PAGE_SIZE = 10;

type SortKey = 'createdAt-desc' | 'createdAt-asc';

export function OrdersListPage() {
  const t = useTranslations('orders');
  const tCommon = useTranslations('common.actions');
  const tStatus = useTranslations('commerce.orderStatus');
  const router = useRouter();
  const queryClient = useQueryClient();
  useOrderRealtimeUpdates();

  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<OrderStatus | undefined>(undefined);
  const [phone, setPhone] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const [sort, setSort] = useState<SortKey>('createdAt-desc');
  const [page, setPage] = useState(1);
  const [rowSelection, setRowSelection] = useState<RowSelectionState>({});

  const { data, isLoading } = useOrdersSearchQuery({
    keyword: search || undefined,
    status,
    phone: phone || undefined,
    createdFrom: dateFrom ? new Date(dateFrom).toISOString() : undefined,
    createdTo: dateTo ? new Date(`${dateTo}T23:59:59.999`).toISOString() : undefined,
    sortBy: 'createdAt',
    sortDescending: sort === 'createdAt-desc',
    page,
    pageSize: PAGE_SIZE,
  });

  const statusOptions = useMemo(
    () =>
      (Object.values(OrderStatus) as OrderStatus[]).map((value) => ({
        value: String(value),
        label: tStatus(ORDER_STATUS_LABEL_KEYS[value]),
      })),
    [tStatus],
  );

  const sortOptions: { value: SortKey; label: string }[] = [
    { value: 'createdAt-desc', label: t('sortOptions.createdAtDesc') },
    { value: 'createdAt-asc', label: t('sortOptions.createdAtAsc') },
  ];

  const selectedCount = Object.values(rowSelection).filter(Boolean).length;

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

      <EntityToolbar
        onSearchChange={(value) => {
          setSearch(value);
          setPage(1);
        }}
        searchPlaceholder={t('searchPlaceholder')}
        filters={
          <FilterPanel active={Boolean(status !== undefined || phone || dateFrom || dateTo)}>
            <div className="space-y-3">
              <div className="space-y-1.5">
                <label className="text-sm font-medium">{t('filters.status')}</label>
                <AppSelect
                  options={statusOptions}
                  value={status !== undefined ? String(status) : undefined}
                  onValueChange={(v) => {
                    setStatus(v ? (Number(v) as OrderStatus) : undefined);
                    setPage(1);
                  }}
                  placeholder={t('anyStatus')}
                />
              </div>
              <div className="space-y-1.5">
                <label className="text-sm font-medium">{t('filters.phone')}</label>
                <AppInput
                  value={phone}
                  onChange={(e) => {
                    setPhone(e.target.value);
                    setPage(1);
                  }}
                  placeholder={t('filters.phonePlaceholder')}
                />
              </div>
              <div className="grid grid-cols-2 gap-2">
                <div className="space-y-1.5">
                  <label className="text-sm font-medium">{t('filters.dateFrom')}</label>
                  <AppInput
                    type="date"
                    value={dateFrom}
                    onChange={(e) => {
                      setDateFrom(e.target.value);
                      setPage(1);
                    }}
                  />
                </div>
                <div className="space-y-1.5">
                  <label className="text-sm font-medium">{t('filters.dateTo')}</label>
                  <AppInput
                    type="date"
                    value={dateTo}
                    onChange={(e) => {
                      setDateTo(e.target.value);
                      setPage(1);
                    }}
                  />
                </div>
              </div>
              <div className="space-y-1.5">
                <label className="text-sm font-medium">{t('filters.sort')}</label>
                <AppSelect
                  options={sortOptions}
                  value={sort}
                  onValueChange={(v) => setSort(v as SortKey)}
                />
              </div>
            </div>
          </FilterPanel>
        }
        selectionBar={<SelectionPanel count={selectedCount} onClear={() => setRowSelection({})} />}
      />

      <AppDataTable<SearchOrdersItemResponse>
        columns={columns}
        data={data?.items ?? []}
        page={page}
        pageCount={data?.totalPages ?? 1}
        onPageChange={setPage}
        isLoading={isLoading}
        enableRowSelection
        rowSelection={rowSelection}
        onRowSelectionChange={setRowSelection}
        getRowId={(row) => row.id}
        rowActions={(order) => (
          <IconButton
            aria-label={tCommon('view')}
            onClick={() => router.push(`/orders/${order.id}`)}
          >
            <Eye />
          </IconButton>
        )}
      />
    </div>
  );
}
