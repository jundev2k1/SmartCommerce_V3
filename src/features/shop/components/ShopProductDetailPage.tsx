'use client';

import { useMemo, useState } from 'react';
import { useTranslations } from 'next-intl';
import { AppLoading, AppEmpty, AppBadge, toast } from '@/shared/ui';
import { EntityDetailHeader } from '@/shared/entity';
import { ProductPrice, VariantSelector, QuantitySelector } from '@/shared/commerce';
import { PrimaryButton } from '@/shared/ui';
import { useProductQuery } from '@/features/products';
import { useCategoriesTreeQuery, flattenTree } from '@/features/categories';
import { useTagsQuery } from '@/features/tags';
import { useAddCartItemMutation } from '@/features/cart';

export function ShopProductDetailPage({ productId }: { productId: string }) {
  const t = useTranslations('shop.detail');
  const tNav = useTranslations('nav');
  const { data: product, isLoading } = useProductQuery(productId);
  const { data: categoryTree } = useCategoriesTreeQuery();
  const { data: tags } = useTagsQuery();
  const addItemMutation = useAddCartItemMutation();

  const variations = useMemo(() => product?.variations ?? [], [product]);
  const [variationId, setVariationId] = useState<string | undefined>(
    () => variations.find((v) => v.isDefault)?.id ?? variations[0]?.id,
  );
  const [quantity, setQuantity] = useState(1);

  if (isLoading) {
    return <AppLoading />;
  }

  if (!product) {
    return <AppEmpty title={t('notFound')} />;
  }

  const selectedVariation = variations.find((v) => v.id === variationId) ?? variations[0];
  const categoryNames = flattenTree(categoryTree ?? [])
    .filter((c) => product.categoryIds?.includes(c.id))
    .map((c) => c.name ?? c.code ?? c.id);
  const tagNames = (tags ?? [])
    .filter((tag) => product.tagIds?.includes(tag.id))
    .map((tag) => tag.name ?? tag.code ?? tag.id);

  async function handleAddToCart() {
    if (!selectedVariation) return;
    try {
      await addItemMutation.mutateAsync({ variationId: selectedVariation.id, quantity });
      toast.success(t('addedToCart'));
    } catch {
      toast.error(t('addToCartError'));
    }
  }

  return (
    <div className="space-y-4">
      <EntityDetailHeader
        breadcrumbItems={[
          { label: tNav('dashboard'), href: '/' },
          { label: tNav('items.shop'), href: '/shop' },
          { label: product.name ?? product.code ?? product.id },
        ]}
        title={product.name ?? product.code ?? t('untitled')}
      />

      <div className="grid gap-6 lg:grid-cols-2">
        <div className="space-y-3">
          {selectedVariation?.images?.length ? (
            <div className="grid grid-cols-3 gap-2">
              {selectedVariation.images.map((url) => (
                // eslint-disable-next-line @next/next/no-img-element -- arbitrary external URL
                <img
                  key={url}
                  src={url}
                  alt={product.name ?? ''}
                  className="aspect-square w-full rounded-md object-cover"
                />
              ))}
            </div>
          ) : (
            <div className="bg-muted aspect-square w-full rounded-md" />
          )}
        </div>

        <div className="space-y-4">
          <p className="text-muted-foreground text-sm">{product.description}</p>

          <p className="text-2xl font-semibold">
            <ProductPrice value={selectedVariation?.price} />
          </p>

          <div className="flex flex-wrap gap-1">
            {categoryNames.map((name) => (
              <AppBadge key={name} variant="secondary">
                {name}
              </AppBadge>
            ))}
            {tagNames.map((name) => (
              <AppBadge key={name} variant="outline">
                {name}
              </AppBadge>
            ))}
          </div>

          <div className="space-y-1.5">
            <label className="text-sm font-medium">{t('variant')}</label>
            <VariantSelector
              variations={variations}
              value={variationId}
              onChange={setVariationId}
            />
          </div>

          <div className="space-y-1.5">
            <label className="text-sm font-medium">{t('quantity')}</label>
            <QuantitySelector value={quantity} onChange={setQuantity} />
          </div>

          <PrimaryButton disabled={!selectedVariation} onClick={handleAddToCart}>
            {t('addToCart')}
          </PrimaryButton>
        </div>
      </div>
    </div>
  );
}
