'use client';

import { useMemo, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import type { ColumnDef } from '@tanstack/react-table';
import { AppDataTable, AppEmpty, IconButton } from '@/shared/ui';
import { EntityHeader, EntityToolbar } from '@/shared/entity';
import { InventoryFilters, StockStatusBadge } from '@/shared/inventory';
import { Eye } from 'lucide-react';
import {
  useLocalInventoryQuery,
  useLocalWarehousesQuery,
  useTransactionsForInventoryIds,
  type InventoryTransactionWithSource,
} from '../api/inventory.queries';

const PAGE_SIZE = 20;

/**
 * Aggregates GetInventoryHistory across every inventory id known to this
 * browser — no "list all transactions" endpoint exists. See
 * docs/modules/inventory-management.md.
 */
export function StockTransactionsPage() {
  const t = useTranslations('inventory.transactions');
  const router = useRouter();
  const { records, hasAny } = useLocalInventoryQuery();
  const { warehouses } = useLocalWarehousesQuery();
  const warehouseOptions = warehouses.map((w) => ({ id: w.id, label: w.name ?? w.code ?? w.id }));

  const [filters, setFilters] = useState<{ warehouseId?: string; type?: string }>({});
  const [page, setPage] = useState(1);

  const scopedIds = useMemo(() => {
    if (!filters.warehouseId) return records.map((r) => r.id);
    return records.filter((r) => r.warehouseId === filters.warehouseId).map((r) => r.id);
  }, [records, filters.warehouseId]);

  const { transactions, isLoading } = useTransactionsForInventoryIds(scopedIds);

  const filteredTransactions = useMemo(() => {
    if (!filters.type) return transactions;
    return transactions.filter((tx) => String(tx.type) === filters.type);
  }, [transactions, filters.type]);

  const typeOptions = useMemo(() => {
    const values = new Set(transactions.map((tx) => String(tx.type)));
    return Array.from(values).map((value) => ({ value, label: t('typeOption', { value }) }));
  }, [transactions, t]);

  const pageCount = Math.max(1, Math.ceil(filteredTransactions.length / PAGE_SIZE));
  const pageItems = filteredTransactions.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  const columns: ColumnDef<InventoryTransactionWithSource>[] = [
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

      <AppDataTable<InventoryTransactionWithSource>
        columns={columns}
        data={pageItems}
        page={page}
        pageCount={pageCount}
        onPageChange={setPage}
        isLoading={isLoading}
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
