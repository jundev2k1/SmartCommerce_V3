'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import { ArrowUp, ArrowDown, Pencil, Star } from 'lucide-react';
import {
  AppTable,
  AppTableHeader,
  AppTableBody,
  AppTableRow,
  AppTableHead,
  AppTableCell,
  CreateButton,
  IconButton,
  DeleteButton,
  AppBadge,
  AppTooltip,
  toast,
} from '@/shared/ui';
import { EntityStatusBadge, ConfirmDeleteDialog } from '@/shared/entity';
import type { GetProductResponse, ProductVariationResponse } from '@/services/product';
import {
  useDeleteVariationMutation,
  useReorderVariationsMutation,
  useSetDefaultVariationMutation,
} from '../api/products.queries';
import { VariantFormDialog, type VariantFormDialogState } from './VariantFormDialog';

export function ProductVariantsTab({ product }: { product: GetProductResponse }) {
  const t = useTranslations('products.variants');
  const tToast = useTranslations('products.toast');
  const tCommon = useTranslations('common.actions');
  const tConfirmDelete = useTranslations('entity.confirmDelete');
  const deleteMutation = useDeleteVariationMutation();
  const reorderMutation = useReorderVariationsMutation();
  const setDefaultMutation = useSetDefaultVariationMutation();

  const [formState, setFormState] = useState<VariantFormDialogState | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<ProductVariationResponse | null>(null);

  const variations = [...(product.variations ?? [])].sort(
    (a, b) => a.displayOrder - b.displayOrder,
  );
  const isLast = variations.length <= 1;

  async function handleDeleteConfirm() {
    if (!deleteTarget) return;
    try {
      await deleteMutation.mutateAsync({ productId: product.id, variationId: deleteTarget.id });
      toast.success(tToast('variationDeleted'));
      setDeleteTarget(null);
    } catch {
      toast.error(tToast('deleteError'));
    }
  }

  async function move(index: number, direction: -1 | 1) {
    const next = [...variations];
    const target = index + direction;
    if (target < 0 || target >= next.length) return;
    [next[index], next[target]] = [next[target], next[index]];
    try {
      await reorderMutation.mutateAsync({
        productId: product.id,
        orderedVariationIds: next.map((v) => v.id),
      });
      toast.success(tToast('variationReordered'));
    } catch {
      toast.error(tToast('updateError'));
    }
  }

  async function handleSetDefault(variationId: string) {
    try {
      await setDefaultMutation.mutateAsync({ productId: product.id, variationId });
      toast.success(tToast('defaultVariationSet'));
    } catch {
      toast.error(tToast('updateError'));
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex justify-end">
        <CreateButton onClick={() => setFormState({ mode: 'create', productId: product.id })}>
          {t('addButton')}
        </CreateButton>
      </div>

      <div className="rounded-md border">
        <AppTable>
          <AppTableHeader>
            <AppTableRow>
              <AppTableHead>{t('table.sku')}</AppTableHead>
              <AppTableHead>{t('table.price')}</AppTableHead>
              <AppTableHead>{t('table.status')}</AppTableHead>
              <AppTableHead>{t('table.default')}</AppTableHead>
              <AppTableHead className="text-right">Actions</AppTableHead>
            </AppTableRow>
          </AppTableHeader>
          <AppTableBody>
            {variations.map((variation, index) => (
              <AppTableRow key={variation.id}>
                <AppTableCell>{variation.sku}</AppTableCell>
                <AppTableCell>{variation.price.toFixed(2)}</AppTableCell>
                <AppTableCell>
                  <EntityStatusBadge status={variation.status} />
                </AppTableCell>
                <AppTableCell>
                  {variation.isDefault ? (
                    <AppBadge>{t('table.default')}</AppBadge>
                  ) : (
                    <IconButton
                      aria-label={t('setDefault')}
                      onClick={() => handleSetDefault(variation.id)}
                    >
                      <Star />
                    </IconButton>
                  )}
                </AppTableCell>
                <AppTableCell>
                  <div className="flex items-center justify-end gap-1">
                    <IconButton
                      aria-label={tCommon('moveUp')}
                      disabled={index === 0}
                      onClick={() => move(index, -1)}
                    >
                      <ArrowUp />
                    </IconButton>
                    <IconButton
                      aria-label={tCommon('moveDown')}
                      disabled={index === variations.length - 1}
                      onClick={() => move(index, 1)}
                    >
                      <ArrowDown />
                    </IconButton>
                    <IconButton
                      aria-label={tCommon('edit')}
                      onClick={() =>
                        setFormState({ mode: 'edit', productId: product.id, variation })
                      }
                    >
                      <Pencil />
                    </IconButton>
                    {isLast ? (
                      <AppTooltip content={t('lastVariantNotice')}>
                        <span>
                          <DeleteButton size="icon" aria-label={tCommon('delete')} disabled />
                        </span>
                      </AppTooltip>
                    ) : (
                      <DeleteButton
                        size="icon"
                        aria-label={tCommon('delete')}
                        onClick={() => setDeleteTarget(variation)}
                      />
                    )}
                  </div>
                </AppTableCell>
              </AppTableRow>
            ))}
          </AppTableBody>
        </AppTable>
      </div>

      <VariantFormDialog state={formState} onOpenChange={(open) => !open && setFormState(null)} />

      <ConfirmDeleteDialog
        open={deleteTarget !== null}
        onOpenChange={(open) => !open && setDeleteTarget(null)}
        onConfirm={handleDeleteConfirm}
        isPending={deleteMutation.isPending}
        title={tConfirmDelete('title', { name: deleteTarget?.sku ?? '' })}
      />
    </div>
  );
}
