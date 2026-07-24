'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import {
  AppSearchBox,
  AppEmpty,
  AppLoading,
  AppPagination,
  PrimaryButton,
  toast,
} from '@/shared/ui';
import { EntityHeader, FilterPanel } from '@/shared/entity';
import { ProductGrid, ProductCard } from '@/shared/commerce';
import { useSearchProductsQuery, ProductFilters } from '@/features/products';
import { useAddCartItemMutation } from '@/features/cart';
import type { SearchProductsItemResponse, SearchProductsParams } from '@/services/product';

const PAGE_SIZE = 12;

/**
 * Customer-facing catalog browse experience — reuses features/products'
 * search/filter pieces (per docs/modules/product-search.md's stated intent)
 * but adds an "Add to cart" action per card, which the admin browse page
 * doesn't have. See docs/modules/client-mock.md.
 */
export function ShopSearchPage() {
  const t = useTranslations('shop');
  const [params, setParams] = useState<SearchProductsParams>({ page: 1, pageSize: PAGE_SIZE });
  const { data, isLoading, isError } = useSearchProductsQuery(params);
  const addItemMutation = useAddCartItemMutation();

  async function handleAddToCart(product: SearchProductsItemResponse) {
    if (!product.defaultVariationId) return;
    try {
      await addItemMutation.mutateAsync({ variationId: product.defaultVariationId, quantity: 1 });
      toast.success(t('addedToCart'));
    } catch {
      toast.error(t('addToCartError'));
    }
  }

  return (
    <div className="space-y-4">
      <EntityHeader title={t('title')} description={t('description')} />

      <div className="flex flex-wrap items-center gap-2">
        <AppSearchBox
          onValueChange={(search) => setParams((p) => ({ ...p, search, page: 1 }))}
          placeholder={t('searchPlaceholder')}
          className="w-full sm:max-w-sm"
        />
        <FilterPanel active={Boolean(params.categoryId || params.tagId || params.status)}>
          <ProductFilters value={params} onChange={(next) => setParams({ ...next, page: 1 })} />
        </FilterPanel>
      </div>

      {isLoading ? (
        <AppLoading />
      ) : isError ? (
        <AppEmpty title={t('errorTitle')} description={t('errorDescription')} />
      ) : !data || data.items.length === 0 ? (
        <AppEmpty title={t('emptyTitle')} description={t('emptyDescription')} />
      ) : (
        <>
          <ProductGrid
            items={data.items}
            getKey={(product) => product.id}
            renderItem={(product) => (
              <ProductCard
                product={product}
                href={`/shop/${product.id}`}
                actions={
                  <PrimaryButton
                    className="w-full"
                    disabled={!product.defaultVariationId || product.isInStock === false}
                    onClick={() => handleAddToCart(product)}
                  >
                    {t('addToCart')}
                  </PrimaryButton>
                }
              />
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
