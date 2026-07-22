'use client';

import { useTranslations } from 'next-intl';
import { AppCheckbox, AppEmpty, toast } from '@/shared/ui';
import { useCategoriesTreeQuery, flattenTree } from '@/features/categories';
import { useAssignCategoryMutation, useRemoveCategoryMutation } from '../api/products.queries';
import type { GetProductResponse } from '@/services/product';

export function ProductCategoriesTab({ product }: { product: GetProductResponse }) {
  const t = useTranslations('products.categoriesTab');
  const tToast = useTranslations('products.toast');
  const { data: tree } = useCategoriesTreeQuery();
  const assignMutation = useAssignCategoryMutation();
  const removeMutation = useRemoveCategoryMutation();

  const categories = flattenTree(tree ?? []);
  const assignedIds = new Set(product.categoryIds ?? []);

  async function toggle(categoryId: string, checked: boolean) {
    try {
      if (checked) {
        await assignMutation.mutateAsync({ productId: product.id, categoryId });
        toast.success(tToast('categoryAssigned'));
      } else {
        await removeMutation.mutateAsync({ productId: product.id, categoryId });
        toast.success(tToast('categoryRemoved'));
      }
    } catch {
      toast.error(tToast('updateError'));
    }
  }

  if (categories.length === 0) {
    return <AppEmpty description={t('none')} />;
  }

  return (
    <div className="space-y-3">
      <p className="text-muted-foreground text-sm">{t('description')}</p>
      <div className="space-y-2">
        {categories.map((category) => (
          <label key={category.id} className="flex items-center gap-2 text-sm">
            <AppCheckbox
              checked={assignedIds.has(category.id)}
              onCheckedChange={(checked) => toggle(category.id, Boolean(checked))}
            />
            {category.name}
          </label>
        ))}
      </div>
    </div>
  );
}
