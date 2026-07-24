'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import type { ColumnDef } from '@tanstack/react-table';
import { AppDataTable, IconButton } from '@/shared/ui';
import { EntityHeader, EntityToolbar } from '@/shared/entity';
import { InventoryFilters, StockStatusBadge } from '@/shared/inventory';
import { Eye } from 'lucide-react';
import type { SearchInventoryTransactionsItemResponse } from '@/services/inventory';
import {
  useWarehousesSearchQuery,
  useInventoryTransactionsSearchQuery,
} from '../api/inventory.queries';

const PAGE_SIZE = 20;
const WAREHOUSE_OPTIONS_PAGE_SIZE = 100;

// InventoryTransactionType is a documented-but-unmapped 3-value enum
// (see services/inventory/types/inventory-transaction-type.ts) — the filter
// options are these opaque values, not derived from loaded data, since a
// single search page can no longer stand in for "every type seen so far".
const TRANSACTION_TYPE_VALUES = ['1', '2', '3'];

export function StockTransactionsPage() {
  const t = useTranslations('inventory.transactions');
  const router = useRouter();
  const { data: warehousesResult } = useWarehousesSearchQuery({
    pageSize: WAREHOUSE_OPTIONS_PAGE_SIZE,
  });
  const warehouseOptions = (warehousesResult?.items ?? []).map((w) => ({
    id: w.id,
    label: w.name ?? w.code ?? w.id,
  }));

  const [filters, setFilters] = useState<{ warehouseId?: string; type?: string }>({});
  const [page, setPage] = useState(1);

  const { data: transactionsResult, isLoading } = useInventoryTransactionsSearchQuery({
    warehouseId: filters.warehouseId,
    type: filters.type,
    page,
    pageSize: PAGE_SIZE,
  });

  const typeOptions = TRANSACTION_TYPE_VALUES.map((value) => ({
    value,
    label: t('typeOption', { value }),
  }));

  const columns: ColumnDef<SearchInventoryTransactionsItemResponse>[] = [
    {
      id: 'type',
      header: t('table.type'),
      cell: ({ row }) => <StockStatusBadge value={row.original.type} />,
    },
    { id: 'inventoryId', header: t('table.inventoryId'), accessorKey: 'inventoryId' },
    { id: 'quantity', header: t('table.quantityChanged'), accessorKey: 'quantity' },
    { id: 'quantityAfter', header: t('table.quantityAfter'), accessorKey: 'quantityAfter' },
    { id: 'reason', header: t('table.reason'), cell: ({ row }) => row.original.reason ?? '—' },
    {
      id: 'createdAt',
      header: t('table.createdAt'),
      cell: ({ row }) => new Date(row.original.createdAt).toLocaleString(),
    },
  ];

  return (
    <div className="space-y-4">
      <EntityHeader title={t('title')} description={t('description')} />

      <EntityToolbar
        filters={
          <InventoryFilters
            value={filters}
            onChange={(next) => {
              setFilters(next);
              setPage(1);
            }}
            warehouses={warehouseOptions}
            typeOptions={typeOptions}
          />
        }
      />

      <AppDataTable<SearchInventoryTransactionsItemResponse>
        columns={columns}
        data={transactionsResult?.items ?? []}
        page={page}
        pageCount={transactionsResult?.totalPages ?? 1}
        onPageChange={setPage}
        isLoading={isLoading}
        emptyTitle={t('emptyTitle')}
        emptyDescription={t('emptyDescription')}
        rowActions={(tx) => (
          <IconButton
            aria-label={t('actions.viewRecord')}
            onClick={() => router.push(`/inventory/${tx.inventoryId}`)}
          >
            <Eye />
          </IconButton>
        )}
      />
    </div>
  );
}
