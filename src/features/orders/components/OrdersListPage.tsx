'use client';

import { useMemo, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { useQueryClient } from '@tanstack/react-query';
import type { ColumnDef, RowSelectionState } from '@tanstack/react-table';
import {
  AppDataTable,
  AppEmpty,
  AppInput,
  AppSelect,
  IconButton,
  PrimaryButton,
  RefreshButton,
} from '@/shared/ui';
import { EntityHeader, EntityToolbar, FilterPanel, SelectionPanel } from '@/shared/entity';
import { OrderStatusBadge, ProductPrice, ORDER_STATUS_LABEL_KEYS } from '@/shared/commerce';
import { Eye } from 'lucide-react';
import { OrderStatus, type GetOrderResponse } from '@/services/order';
import { useLocalOrdersQuery, orderKeys } from '../api/orders.queries';
import { useOrderRealtimeUpdates } from '../hooks/useOrderRealtimeUpdates';

const PAGE_SIZE = 10;

type SortKey = 'createdAt-desc' | 'createdAt-asc' | 'total-desc' | 'total-asc';

export function OrdersListPage() {
  const t = useTranslations('orders');
  const tCommon = useTranslations('common.actions');
  const tStatus = useTranslations('commerce.orderStatus');
  const router = useRouter();
  const queryClient = useQueryClient();
  const { orders, isLoading, hasAny } = useLocalOrdersQuery();
  // No monolithic order-list query exists to invalidate — a live event just
  // invalidates its one order's detail query, which this page's underlying
  // useQueries picks up automatically on next render.
  useOrderRealtimeUpdates();

  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<string | undefined>(undefined);
  const [customerId, setCustomerId] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const [sort, setSort] = useState<SortKey>('createdAt-desc');
  const [page, setPage] = useState(1);
  const [rowSelection, setRowSelection] = useState<RowSelectionState>({});

  const filtered = useMemo(() => {
    let result = orders;
    if (search.trim()) {
      const query = search.trim().toLowerCase();
      result = result.filter((order) => order.id.toLowerCase().includes(query));
    }
    if (status) {
      result = result.filter((order) => String(order.status) === status);
    }
    if (customerId.trim()) {
      const query = customerId.trim().toLowerCase();
      result = result.filter((order) => order.customerId.toLowerCase().includes(query));
    }
    if (dateFrom) {
      const from = new Date(dateFrom).getTime();
      result = result.filter((order) => new Date(order.createdAt).getTime() >= from);
    }
    if (dateTo) {
      const to = new Date(dateTo).getTime();
      result = result.filter((order) => new Date(order.createdAt).getTime() <= to);
    }
    const sorted = [...result];
    sorted.sort((a, b) => {
      switch (sort) {
        case 'createdAt-asc':
          return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
        case 'total-desc':
          return b.totalAmount - a.totalAmount;
        case 'total-asc':
          return a.totalAmount - b.totalAmount;
        case 'createdAt-desc':
        default:
          return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
      }
    });
    return sorted;
  }, [orders, search, status, customerId, dateFrom, dateTo, sort]);

  const pageCount = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const pageItems = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  const statusOptions = useMemo(() => {
    const values = new Set(orders.map((order) => order.status));
    return Array.from(values).map((value) => ({
      value: String(value),
      label:
        value in ORDER_STATUS_LABEL_KEYS
          ? tStatus(ORDER_STATUS_LABEL_KEYS[value as OrderStatus])
          : t('statusOption', { value }),
    }));
  }, [orders, t, tStatus]);

  const sortOptions: { value: SortKey; label: string }[] = [
    { value: 'createdAt-desc', label: t('sortOptions.createdAtDesc') },
    { value: 'createdAt-asc', label: t('sortOptions.createdAtAsc') },
    { value: 'total-desc', label: t('sortOptions.totalDesc') },
    { value: 'total-asc', label: t('sortOptions.totalAsc') },
  ];

  const selectedCount = Object.values(rowSelection).filter(Boolean).length;

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
        <AppEmpty
          title={t('noLocalOrdersTitle')}
          description={t('noLocalOrdersDescription')}
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
          <FilterPanel active={Boolean(status || customerId || dateFrom || dateTo)}>
            <div className="space-y-3">
              <div className="space-y-1.5">
                <label className="text-sm font-medium">{t('filters.status')}</label>
                <AppSelect
                  options={statusOptions}
                  value={status}
                  onValueChange={(v) => {
                    setStatus(v);
                    setPage(1);
                  }}
                  placeholder={t('anyStatus')}
                />
              </div>
              <div className="space-y-1.5">
                <label className="text-sm font-medium">{t('filters.customerId')}</label>
                <AppInput
                  value={customerId}
                  onChange={(e) => {
                    setCustomerId(e.target.value);
                    setPage(1);
                  }}
                  placeholder={t('filters.customerIdPlaceholder')}
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

      <AppDataTable<GetOrderResponse>
        columns={columns}
        data={pageItems}
        page={page}
        pageCount={pageCount}
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
