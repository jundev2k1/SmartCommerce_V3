'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import type { ColumnDef } from '@tanstack/react-table';
import { AppDataTable, AppTooltip, CreateButton, DeleteButton, IconButton } from '@/shared/ui';
import { EntityHeader, EntityStatusBadge } from '@/shared/entity';
import { InventoryToolbar } from '@/shared/inventory';
import { Eye, Pencil } from 'lucide-react';
import type { GetWarehouseResponse } from '@/services/inventory';
import { useWarehousesSearchQuery } from '../api/inventory.queries';
import { WarehouseCreateDialog } from './WarehouseCreateDialog';

const PAGE_SIZE = 20;

export function WarehousesListPage() {
  const t = useTranslations('inventory.warehouses');
  const router = useRouter();

  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [createOpen, setCreateOpen] = useState(false);

  const { data, isLoading } = useWarehousesSearchQuery({ search, page, pageSize: PAGE_SIZE });

  const columns: ColumnDef<GetWarehouseResponse>[] = [
    { accessorKey: 'code', header: t('table.code') },
    { accessorKey: 'name', header: t('table.name') },
    { accessorKey: 'address', header: t('table.address') },
    {
      id: 'status',
      header: t('table.status'),
      cell: ({ row }) => <EntityStatusBadge status={row.original.status} />,
    },
  ];

  return (
    <div className="space-y-4">
      <EntityHeader
        title={t('title')}
        actions={
          <CreateButton onClick={() => setCreateOpen(true)}>{t('createButton')}</CreateButton>
        }
      />

      <InventoryToolbar
        onSearchChange={(value) => {
          setSearch(value);
          setPage(1);
        }}
        searchPlaceholder={t('searchPlaceholder')}
        onOpenById={(id) => router.push(`/warehouses/${id}`)}
      />

      <AppDataTable<GetWarehouseResponse>
        columns={columns}
        data={data?.items ?? []}
        page={page}
        pageCount={data?.totalPages ?? 1}
        onPageChange={setPage}
        isLoading={isLoading}
        emptyTitle={t('emptyTitle')}
        emptyDescription={t('emptyDescription')}
        rowActions={(warehouse) => (
          <div className="flex items-center justify-end gap-1">
            <IconButton
              aria-label={t('actions.view')}
              onClick={() => router.push(`/warehouses/${warehouse.id}`)}
            >
              <Eye />
            </IconButton>
            <AppTooltip content={t('updateNotAvailable')}>
              <span>
                <IconButton disabled aria-label={t('actions.edit')}>
                  <Pencil />
                </IconButton>
              </span>
            </AppTooltip>
            <AppTooltip content={t('deleteNotAvailable')}>
              <span>
                <DeleteButton disabled size="icon" aria-label={t('actions.delete')} />
              </span>
            </AppTooltip>
          </div>
        )}
      />

      <WarehouseCreateDialog open={createOpen} onOpenChange={setCreateOpen} />
    </div>
  );
}
