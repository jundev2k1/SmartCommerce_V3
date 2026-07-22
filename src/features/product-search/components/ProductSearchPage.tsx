'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import { AppSearchBox, AppEmpty, AppLoading, AppPagination } from '@/shared/ui';
import { EntityHeader, FilterPanel } from '@/shared/entity';
import { ProductGrid, ProductCard } from '@/shared/commerce';
import { useSearchProductsQuery, ProductFilters } from '@/features/products';
import type { SearchProductsParams } from '@/services/product';

const PAGE_SIZE = 12;

/**
 * Card-grid browse experience over the same SearchProducts endpoint and
 * filter pieces the admin Products table uses — demonstrates the reusable
 * search UI a future customer-facing Client page would reuse, rather than
 * duplicating the admin table. See docs/modules/product-search.md.
 *
 * Cards link to the admin Product detail page (`/products/[id]`) — this is
 * the admin-facing browse experience, unlike features/shop's customer-facing
 * one, which links to `/shop/[id]` and adds an "Add to cart" action instead.
 */
export function ProductSearchPage() {
  const t = useTranslations('productSearch');
  const [params, setParams] = useState<SearchProductsParams>({ page: 1, pageSize: PAGE_SIZE });
  const { data, isLoading } = useSearchProductsQuery(params);

  return (
    <div className="space-y-4">
      <EntityHeader title={t('title')} />

      <div className="flex items-center gap-2">
        <AppSearchBox
          onValueChange={(search) => setParams((p) => ({ ...p, search, page: 1 }))}
          placeholder={t('searchPlaceholder')}
          className="max-w-sm"
        />
        <FilterPanel active={Boolean(params.categoryId || params.tagId || params.status)}>
          <ProductFilters value={params} onChange={(next) => setParams({ ...next, page: 1 })} />
        </FilterPanel>
      </div>

      {isLoading ? (
        <AppLoading />
      ) : !data || data.items.length === 0 ? (
        <AppEmpty description={t('empty')} />
      ) : (
        <>
          <ProductGrid
            items={data.items}
            getKey={(product) => product.id}
            renderItem={(product) => (
              <ProductCard product={product} href={`/products/${product.id}`} />
            )}
          />
          <AppPagination
            page={params.page ?? 1}
            pageCount={data.totalPages}
            onPageChange={(page) => setParams((p) => ({ ...p, page }))}
          />
        </>
      )}
    </div>
  );
}
