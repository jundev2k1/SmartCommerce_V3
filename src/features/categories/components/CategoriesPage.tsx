'use client';

import { useMemo, useState } from 'react';
import { useTranslations } from 'next-intl';
import { CreateButton, AppSearchBox, AppLoading, AppEmpty, toast } from '@/shared/ui';
import { EntityHeader, ConfirmDeleteDialog } from '@/shared/entity';
import { useCategoriesTreeQuery, useDeleteCategoryMutation } from '../api/categories.queries';
import {
  collectDescendantIds,
  flattenTree,
  type CategoryTreeNode as TreeNode,
} from '../categories.utils';
import { CategoryTree } from './CategoryTree';
import { CategoryFormDialog, type CategoryFormDialogState } from './CategoryFormDialog';
import { CategoryDetailDrawer } from './CategoryDetailDrawer';

export function CategoriesPage() {
  const t = useTranslations('categories');
  const tConfirmDelete = useTranslations('entity.confirmDelete');
  const { data: tree, isLoading } = useCategoriesTreeQuery();
  const deleteMutation = useDeleteCategoryMutation();

  const [search, setSearch] = useState('');
  const [formState, setFormState] = useState<CategoryFormDialogState | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<TreeNode | null>(null);
  const [viewTarget, setViewTarget] = useState<TreeNode | null>(null);

  const allNodes = useMemo(() => flattenTree(tree ?? []), [tree]);

  const visibleNodes = useMemo(() => {
    if (!tree) return [];
    if (!search.trim()) return tree;
    const query = search.trim().toLowerCase();
    return allNodes
      .filter(
        (node) =>
          node.name?.toLowerCase().includes(query) || node.code?.toLowerCase().includes(query),
      )
      .map((node) => ({ ...node, children: [] }));
  }, [tree, allNodes, search]);

  function optionsExcluding(nodeId?: string) {
    const excluded = new Set<string>();
    if (nodeId) {
      excluded.add(nodeId);
      const node = allNodes.find((n) => n.id === nodeId);
      if (node) collectDescendantIds(node).forEach((id) => excluded.add(id));
    }
    return allNodes
      .filter((node) => !excluded.has(node.id))
      .map((node) => ({ value: node.id, label: node.name ?? node.code ?? node.id }));
  }

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

      <AppSearchBox
        onValueChange={setSearch}
        placeholder={t('searchPlaceholder')}
        className="max-w-sm"
      />

      {isLoading ? (
        <AppLoading />
      ) : visibleNodes.length === 0 ? (
        <AppEmpty description={t('empty')} />
      ) : (
        <CategoryTree
          nodes={visibleNodes}
          onView={setViewTarget}
          onEdit={(node) => setFormState({ mode: 'edit', categoryId: node.id })}
          onDelete={setDeleteTarget}
          onAddChild={(node) => setFormState({ mode: 'create', defaultParentId: node.id })}
        />
      )}

      <CategoryFormDialog
        state={formState}
        categoryOptions={optionsExcluding(
          formState?.mode === 'edit' ? formState.categoryId : undefined,
        )}
        onOpenChange={(open) => !open && setFormState(null)}
      />

      <ConfirmDeleteDialog
        open={deleteTarget !== null}
        onOpenChange={(open) => !open && setDeleteTarget(null)}
        onConfirm={handleDeleteConfirm}
        isPending={deleteMutation.isPending}
        title={tConfirmDelete('title', { name: deleteTarget?.name ?? deleteTarget?.code ?? '' })}
      />

      <CategoryDetailDrawer
        node={viewTarget}
        onOpenChange={(open) => !open && setViewTarget(null)}
      />
    </div>
  );
}
