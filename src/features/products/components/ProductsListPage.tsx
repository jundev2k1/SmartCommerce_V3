'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import type { ColumnDef, RowSelectionState } from '@tanstack/react-table';
import { AppDataTable, AppBadge, CreateButton, IconButton, DeleteButton, toast } from '@/shared/ui';
import {
  EntityHeader,
  EntityToolbar,
  EntityStatusBadge,
  FilterPanel,
  SelectionPanel,
  ConfirmDeleteDialog,
} from '@/shared/entity';
import { Pencil } from 'lucide-react';
import type { SearchProductsItemResponse, SearchProductsParams } from '@/services/product';
import { useSearchProductsQuery, useDeleteProductMutation } from '../api/products.queries';
import { ProductFilters } from './ProductFilters';
import { ProductCreateDialog } from './ProductCreateDialog';

const PAGE_SIZE = 20;

export function ProductsListPage() {
  const t = useTranslations('products');
  const tCommon = useTranslations('common.actions');
  const tConfirmDelete = useTranslations('entity.confirmDelete');
  const router = useRouter();
  const deleteMutation = useDeleteProductMutation();

  const [params, setParams] = useState<SearchProductsParams>({ page: 1, pageSize: PAGE_SIZE });
  const [rowSelection, setRowSelection] = useState<RowSelectionState>({});
  const [createOpen, setCreateOpen] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<SearchProductsItemResponse | null>(null);

  const { data, isLoading } = useSearchProductsQuery(params);

  const columns: ColumnDef<SearchProductsItemResponse>[] = [
    { accessorKey: 'name', header: t('table.name') },
    { accessorKey: 'code', header: t('table.code') },
    {
      id: 'price',
      header: t('table.price'),
      cell: ({ row }) =>
        row.original.defaultPrice != null ? row.original.defaultPrice.toFixed(2) : '—',
    },
    {
      id: 'status',
      header: t('table.status'),
      cell: ({ row }) => <EntityStatusBadge status={row.original.status} />,
    },
    {
      id: 'stock',
      header: t('table.stock'),
      cell: ({ row }) =>
        row.original.isInStock === false ? (
          <AppBadge variant="destructive">{t('table.outOfStock')}</AppBadge>
        ) : row.original.isInStock === true ? (
          <AppBadge variant="secondary">{t('table.inStock')}</AppBadge>
        ) : (
          '—'
        ),
    },
    {
      id: 'categories',
      header: t('table.categories'),
      cell: ({ row }) => row.original.categoryNames?.join(', ') || '—',
    },
    {
      id: 'tags',
      header: t('table.tags'),
      cell: ({ row }) => row.original.tagNames?.join(', ') || '—',
    },
    {
      id: 'updatedAt',
      header: t('table.updatedAt'),
      cell: ({ row }) => new Date(row.original.updatedAt).toLocaleDateString(),
    },
  ];

  const selectedCount = Object.values(rowSelection).filter(Boolean).length;

  async function handleDeleteConfirm() {
    if (!deleteTarget) return;
    try {
      await deleteMutation.mutateAsync(deleteTarget.id);
      toast.success(t('toast.deleteSuccess'));
      setDeleteTarget(null);
    } catch {
      toast.error(t('toast.deleteError'));
    }
  }

  return (
    <div className="space-y-4">
      <EntityHeader
        title={t('title')}
        actions={
          <CreateButton onClick={() => setCreateOpen(true)}>{t('createButton')}</CreateButton>
        }
      />

      <EntityToolbar
        onSearchChange={(search) => setParams((p) => ({ ...p, search, page: 1 }))}
        filters={
          <FilterPanel
            active={Boolean(
              params.categoryId ||
              params.tagId ||
              params.status ||
              params.sortBy ||
              params.sortDescending,
            )}
            onClear={() => setParams((p) => ({ search: p.search, page: 1, pageSize: PAGE_SIZE }))}
          >
            <ProductFilters value={params} onChange={(next) => setParams({ ...next, page: 1 })} />
          </FilterPanel>
        }
        selectionBar={<SelectionPanel count={selectedCount} onClear={() => setRowSelection({})} />}
      />

      <AppDataTable<SearchProductsItemResponse>
        columns={columns}
        data={data?.items ?? []}
        page={params.page ?? 1}
        pageCount={data?.totalPages ?? 1}
        onPageChange={(page) => setParams((p) => ({ ...p, page }))}
        isLoading={isLoading}
        enableRowSelection
        rowSelection={rowSelection}
        onRowSelectionChange={setRowSelection}
        getRowId={(row) => row.id}
        rowActions={(product) => (
          <div className="flex items-center justify-end gap-1">
            <IconButton
              aria-label={tCommon('view')}
              onClick={() => router.push(`/products/${product.id}`)}
            >
              <Pencil />
            </IconButton>
            <DeleteButton
              size="icon"
              aria-label={tCommon('delete')}
              onClick={() => setDeleteTarget(product)}
            />
          </div>
        )}
      />

      <ProductCreateDialog open={createOpen} onOpenChange={setCreateOpen} />

      <ConfirmDeleteDialog
        open={deleteTarget !== null}
        onOpenChange={(open) => !open && setDeleteTarget(null)}
        onConfirm={handleDeleteConfirm}
        isPending={deleteMutation.isPending}
        title={tConfirmDelete('title', { name: deleteTarget?.name ?? deleteTarget?.code ?? '' })}
      />
    </div>
  );
}
