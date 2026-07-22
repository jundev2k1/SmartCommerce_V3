'use client';

import { useTranslations } from 'next-intl';
import {
  AppLoading,
  AppEmpty,
  AppTabs,
  AppTabsList,
  AppTabsTrigger,
  AppTabsContent,
} from '@/shared/ui';
import { EntityDetailHeader, EntityMetadata, AuditTrailButton } from '@/shared/entity';
import { useProductQuery } from '../api/products.queries';
import { ProductGeneralTab } from './ProductGeneralTab';
import { ProductVariantsTab } from './ProductVariantsTab';
import { ProductCategoriesTab } from './ProductCategoriesTab';
import { ProductTagsTab } from './ProductTagsTab';

export function ProductDetailPage({ productId }: { productId: string }) {
  const t = useTranslations('products.detail');
  const tNav = useTranslations('nav');
  const { data: product, isLoading } = useProductQuery(productId);

  if (isLoading) {
    return <AppLoading />;
  }

  if (!product) {
    return <AppEmpty title={t('notFound')} />;
  }

  return (
    <div className="space-y-4">
      <EntityDetailHeader
        breadcrumbItems={[
          { label: tNav('dashboard'), href: '/' },
          { label: tNav('items.products'), href: '/products' },
          { label: product.name ?? product.code ?? product.id },
        ]}
        title={product.name ?? product.code ?? t('untitled')}
        actions={<AuditTrailButton service="Product" entityId={product.id} />}
      />

      <AppTabs defaultValue="general">
        <AppTabsList>
          <AppTabsTrigger value="general">{t('tabs.general')}</AppTabsTrigger>
          <AppTabsTrigger value="variants">{t('tabs.variants')}</AppTabsTrigger>
          <AppTabsTrigger value="categories">{t('tabs.categories')}</AppTabsTrigger>
          <AppTabsTrigger value="tags">{t('tabs.tags')}</AppTabsTrigger>
          <AppTabsTrigger value="metadata">{t('tabs.metadata')}</AppTabsTrigger>
        </AppTabsList>
        <AppTabsContent value="general">
          <ProductGeneralTab product={product} />
        </AppTabsContent>
        <AppTabsContent value="variants">
          <ProductVariantsTab product={product} />
        </AppTabsContent>
        <AppTabsContent value="categories">
          <ProductCategoriesTab product={product} />
        </AppTabsContent>
        <AppTabsContent value="tags">
          <ProductTagsTab product={product} />
        </AppTabsContent>
        <AppTabsContent value="metadata">
          <EntityMetadata
            items={[
              { label: t('metadata.id'), value: product.id },
              { label: t('metadata.code'), value: product.code ?? '—' },
              {
                label: t('metadata.createdAt'),
                value: new Date(product.createdAt).toLocaleString(),
              },
              {
                label: t('metadata.updatedAt'),
                value: new Date(product.updatedAt).toLocaleString(),
              },
            ]}
          />
        </AppTabsContent>
      </AppTabs>
    </div>
  );
}
