'use client';

import { useMemo, useState } from 'react';
import { useTranslations } from 'next-intl';
import type { ColumnDef } from '@tanstack/react-table';
import { AppDataTable, CreateButton, IconButton, DeleteButton, toast } from '@/shared/ui';
import { EntityHeader, EntityToolbar, ConfirmDeleteDialog } from '@/shared/entity';
import { Eye, Pencil } from 'lucide-react';
import type { ProductTagItemResponse } from '@/services/product';
import { useTagsQuery, useDeleteTagMutation } from '../api/tags.queries';
import { TagFormDialog, type TagFormDialogState } from './TagFormDialog';
import { TagDetailDrawer } from './TagDetailDrawer';

const PAGE_SIZE = 20;

export function TagsPage() {
  const t = useTranslations('tags');
  const tCommon = useTranslations('common.actions');
  const tConfirmDelete = useTranslations('entity.confirmDelete');
  const { data: tags, isLoading } = useTagsQuery();
  const deleteMutation = useDeleteTagMutation();

  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [formState, setFormState] = useState<TagFormDialogState | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<ProductTagItemResponse | null>(null);
  const [viewTarget, setViewTarget] = useState<ProductTagItemResponse | null>(null);

  const filtered = useMemo(() => {
    const all = tags ?? [];
    if (!search.trim()) return all;
    const query = search.trim().toLowerCase();
    return all.filter(
      (tag) => tag.name?.toLowerCase().includes(query) || tag.code?.toLowerCase().includes(query),
    );
  }, [tags, search]);

  const pageCount = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const pageItems = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  const columns: ColumnDef<ProductTagItemResponse>[] = [
    { accessorKey: 'code', header: t('table.code') },
    { accessorKey: 'name', header: t('table.name') },
  ];

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
          <CreateButton onClick={() => setFormState({ mode: 'create' })}>
            {t('createButton')}
          </CreateButton>
        }
      />

      <EntityToolbar
        onSearchChange={(value) => {
          setSearch(value);
          setPage(1);
        }}
        searchPlaceholder={t('searchPlaceholder')}
      />

      <AppDataTable<ProductTagItemResponse>
        columns={columns}
        data={pageItems}
        page={page}
        pageCount={pageCount}
        onPageChange={setPage}
        isLoading={isLoading}
        rowActions={(tag) => (
          <div className="flex items-center justify-end gap-1">
            <IconButton aria-label={tCommon('view')} onClick={() => setViewTarget(tag)}>
              <Eye />
            </IconButton>
            <IconButton
              aria-label={tCommon('edit')}
              onClick={() =>
                setFormState({
                  mode: 'edit',
                  tagId: tag.id,
                  defaultValues: { name: tag.name ?? '' },
                })
              }
            >
              <Pencil />
            </IconButton>
            <DeleteButton
              size="icon"
              aria-label={tCommon('delete')}
              onClick={() => setDeleteTarget(tag)}
            />
          </div>
        )}
      />

      <TagFormDialog state={formState} onOpenChange={(open) => !open && setFormState(null)} />

      <ConfirmDeleteDialog
        open={deleteTarget !== null}
        onOpenChange={(open) => !open && setDeleteTarget(null)}
        onConfirm={handleDeleteConfirm}
        isPending={deleteMutation.isPending}
        title={tConfirmDelete('title', { name: deleteTarget?.name ?? deleteTarget?.code ?? '' })}
      />

      <TagDetailDrawer tag={viewTarget} onOpenChange={(open) => !open && setViewTarget(null)} />
    </div>
  );
}
