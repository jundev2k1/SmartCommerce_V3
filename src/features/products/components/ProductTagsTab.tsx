'use client';

import { useTranslations } from 'next-intl';
import { AppCheckbox, AppEmpty, toast } from '@/shared/ui';
import { useTagsQuery } from '@/features/tags';
import { useAssignTagMutation, useRemoveTagMutation } from '../api/products.queries';
import type { GetProductResponse } from '@/services/product';

export function ProductTagsTab({ product }: { product: GetProductResponse }) {
  const t = useTranslations('products.tagsTab');
  const tToast = useTranslations('products.toast');
  const { data: tags } = useTagsQuery();
  const assignMutation = useAssignTagMutation();
  const removeMutation = useRemoveTagMutation();

  const assignedIds = new Set(product.tagIds ?? []);

  async function toggle(tagId: string, checked: boolean) {
    try {
      if (checked) {
        await assignMutation.mutateAsync({ productId: product.id, tagId });
        toast.success(tToast('tagAssigned'));
      } else {
        await removeMutation.mutateAsync({ productId: product.id, tagId });
        toast.success(tToast('tagRemoved'));
      }
    } catch {
      toast.error(tToast('updateError'));
    }
  }

  if (!tags || tags.length === 0) {
    return <AppEmpty description={t('none')} />;
  }

  return (
    <div className="space-y-3">
      <p className="text-muted-foreground text-sm">{t('description')}</p>
      <div className="space-y-2">
        {tags.map((tag) => (
          <label key={tag.id} className="flex items-center gap-2 text-sm">
            <AppCheckbox
              checked={assignedIds.has(tag.id)}
              onCheckedChange={(checked) => toggle(tag.id, Boolean(checked))}
            />
            {tag.name}
          </label>
        ))}
      </div>
    </div>
  );
}
