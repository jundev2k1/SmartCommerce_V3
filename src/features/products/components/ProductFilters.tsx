'use client';

import { useTranslations } from 'next-intl';
import { AppSelect, AppSwitch } from '@/shared/ui';
import { useCategoriesTreeQuery, flattenTree } from '@/features/categories';
import { useTagsQuery } from '@/features/tags';
import type { SearchProductsParams } from '@/services/product';

export interface ProductFiltersProps {
  value: SearchProductsParams;
  onChange: (next: SearchProductsParams) => void;
}

export function ProductFilters({ value, onChange }: ProductFiltersProps) {
  const t = useTranslations('products.filters');
  const { data: categoryTree } = useCategoriesTreeQuery();
  const { data: tags } = useTagsQuery();

  const categoryOptions = flattenTree(categoryTree ?? []).map((node) => ({
    value: node.id,
    label: node.name ?? node.code ?? node.id,
  }));
  const tagOptions = (tags ?? []).map((tag) => ({
    value: tag.id,
    label: tag.name ?? tag.code ?? tag.id,
  }));
  const sortOptions = [
    { value: 'name', label: t('sortByOptions.name') },
    { value: 'price', label: t('sortByOptions.price') },
    { value: 'updatedAt', label: t('sortByOptions.updatedAt') },
  ];

  return (
    <div className="space-y-3">
      <div className="space-y-1.5">
        <label className="text-sm font-medium">{t('category')}</label>
        <AppSelect
          options={categoryOptions}
          value={value.categoryId}
          onValueChange={(v) => onChange({ ...value, categoryId: v })}
          placeholder={t('allCategories')}
        />
      </div>
      <div className="space-y-1.5">
        <label className="text-sm font-medium">{t('tag')}</label>
        <AppSelect
          options={tagOptions}
          value={value.tagId}
          onValueChange={(v) => onChange({ ...value, tagId: v })}
          placeholder={t('allTags')}
        />
      </div>
      <div className="space-y-1.5">
        <label className="text-sm font-medium">{t('sortBy')}</label>
        <AppSelect
          options={sortOptions}
          value={value.sortBy}
          onValueChange={(v) => onChange({ ...value, sortBy: v as SearchProductsParams['sortBy'] })}
          placeholder={t('sortBy')}
        />
      </div>
      <div className="flex items-center gap-2">
        <AppSwitch
          checked={Boolean(value.sortDescending)}
          onCheckedChange={(checked) => onChange({ ...value, sortDescending: checked })}
        />
        <label className="text-sm">{t('sortDescending')}</label>
      </div>
    </div>
  );
}
