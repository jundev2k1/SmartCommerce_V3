'use client';

import { useEffect, useState } from 'react';
import { useTranslations } from 'next-intl';
import {
  AppLoading,
  AppEmpty,
  AppTabs,
  AppTabsList,
  AppTabsTrigger,
  AppTabsContent,
  AppTooltip,
  PrimaryButton,
  SecondaryButton,
} from '@/shared/ui';
import { EntityDetailHeader, EntityMetadata, AuditTrailButton } from '@/shared/entity';
import { InventorySummaryCard, TransactionTimeline } from '@/shared/inventory';
import { useProductQuery } from '@/features/products';
import { useLocalInventoryIdsStore } from '@/shared/stores/local-inventory-ids.store';
import {
  useInventoryQuery,
  useInventoryHistoryQuery,
  useWarehouseQuery,
} from '../api/inventory.queries';
import { StockActionDialog, type StockActionMode } from './StockActionDialog';

export function InventoryDetailPage({ inventoryId }: { inventoryId: string }) {
  const t = useTranslations('inventory.stockDetail');
  const tNav = useTranslations('nav');
  const { data: record, isLoading } = useInventoryQuery(inventoryId);
  const { data: history, isLoading: historyLoading } = useInventoryHistoryQuery(inventoryId);
  const { data: product } = useProductQuery(record?.productId ?? '');
  const { data: warehouse } = useWarehouseQuery(record?.warehouseId ?? '');
  const [actionMode, setActionMode] = useState<StockActionMode | null>(null);

  useEffect(() => {
    if (record) {
      useLocalInventoryIdsStore.getState().addInventoryId(record.id);
    }
  }, [record]);

  if (isLoading) {
    return <AppLoading />;
  }

  if (!record) {
    return <AppEmpty title={t('notFound')} />;
  }

  const variation = product?.variations?.find((v) => v.id === record.productVariationId);
  const productName = product?.name ?? product?.code ?? record.productId;
  const variationLabel = variation?.sku ?? record.productVariationId;
  const warehouseName = warehouse?.name ?? warehouse?.code ?? record.warehouseId;

  return (
    <div className="space-y-4">
      <EntityDetailHeader
        breadcrumbItems={[
          { label: tNav('dashboard'), href: '/' },
          { label: tNav('items.inventory'), href: '/inventory' },
          { label: record.id },
        ]}
        title={t('title')}
        actions={<AuditTrailButton service="Inventory" entityId={record.id} />}
      />

      <InventorySummaryCard
        productName={productName}
        variationLabel={variationLabel}
        warehouseName={warehouseName}
        quantity={record.quantity}
      />

      <AppTabs defaultValue="actions">
        <AppTabsList>
          <AppTabsTrigger value="actions">{t('tabs.actions')}</AppTabsTrigger>
          <AppTabsTrigger value="transactions">{t('tabs.transactions')}</AppTabsTrigger>
          <AppTabsTrigger value="metadata">{t('tabs.metadata')}</AppTabsTrigger>
        </AppTabsList>

        <AppTabsContent value="actions">
          <div className="flex flex-wrap gap-2">
            <PrimaryButton onClick={() => setActionMode('in')}>
              {t('actions.stockIn')}
            </PrimaryButton>
            <SecondaryButton onClick={() => setActionMode('out')}>
              {t('actions.stockOut')}
            </SecondaryButton>
            <SecondaryButton onClick={() => setActionMode('adjust')}>
              {t('actions.adjust')}
            </SecondaryButton>
            <AppTooltip content={t('actions.transferNotAvailable')}>
              <span>
                <SecondaryButton disabled>{t('actions.transfer')}</SecondaryButton>
              </span>
            </AppTooltip>
          </div>
        </AppTabsContent>

        <AppTabsContent value="transactions">
          {historyLoading ? (
            <AppLoading />
          ) : (
            <TransactionTimeline transactions={history?.transactions ?? []} />
          )}
        </AppTabsContent>

        <AppTabsContent value="metadata">
          <EntityMetadata
            items={[
              { label: t('metadata.id'), value: record.id },
              { label: t('metadata.productId'), value: record.productId },
              { label: t('metadata.variationId'), value: record.productVariationId },
              { label: t('metadata.warehouseId'), value: record.warehouseId },
              {
                label: t('metadata.createdAt'),
                value: new Date(record.createdAt).toLocaleString(),
              },
              {
                label: t('metadata.updatedAt'),
                value: new Date(record.updatedAt).toLocaleString(),
              },
            ]}
          />
        </AppTabsContent>
      </AppTabs>

      <StockActionDialog
        mode={actionMode}
        inventoryId={record.id}
        onOpenChange={(open) => !open && setActionMode(null)}
      />
    </div>
  );
}
