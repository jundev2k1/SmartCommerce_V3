'use client';

import { useMemo } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import {
  AppLoading,
  AppEmpty,
  AppTable,
  AppTableHeader,
  AppTableBody,
  AppTableRow,
  AppTableHead,
  AppTableCell,
  AppTabs,
  AppTabsList,
  AppTabsTrigger,
  AppTabsContent,
  IconButton,
} from '@/shared/ui';
import { EntityDetailHeader, EntityMetadata, AuditTrailButton } from '@/shared/entity';
import { StockQuantity, TransactionTimeline } from '@/shared/inventory';
import { Eye } from 'lucide-react';
import {
  useWarehouseQuery,
  useLocalInventoryQuery,
  useTransactionsForInventoryIds,
} from '../api/inventory.queries';

export function WarehouseDetailPage({ warehouseId }: { warehouseId: string }) {
  const t = useTranslations('inventory.warehouses.detail');
  const tNav = useTranslations('nav');
  const router = useRouter();
  const { data: warehouse, isLoading } = useWarehouseQuery(warehouseId);
  const { records } = useLocalInventoryQuery();

  const warehouseRecords = useMemo(
    () => records.filter((record) => record.warehouseId === warehouseId),
    [records, warehouseId],
  );
  const { transactions, isLoading: transactionsLoading } = useTransactionsForInventoryIds(
    warehouseRecords.map((r) => r.id),
  );

  if (isLoading) {
    return <AppLoading />;
  }

  if (!warehouse) {
    return <AppEmpty title={t('notFound')} />;
  }

  return (
    <div className="space-y-4">
      <EntityDetailHeader
        breadcrumbItems={[
          { label: tNav('dashboard'), href: '/' },
          { label: tNav('items.warehouses'), href: '/warehouses' },
          { label: warehouse.name ?? warehouse.code ?? warehouse.id },
        ]}
        title={warehouse.name ?? warehouse.code ?? t('untitled')}
        actions={<AuditTrailButton service="Inventory" entityId={warehouse.id} />}
      />

      <AppTabs defaultValue="summary">
        <AppTabsList>
          <AppTabsTrigger value="summary">{t('tabs.summary')}</AppTabsTrigger>
          <AppTabsTrigger value="stock">{t('tabs.stock')}</AppTabsTrigger>
          <AppTabsTrigger value="transactions">{t('tabs.transactions')}</AppTabsTrigger>
          <AppTabsTrigger value="metadata">{t('tabs.metadata')}</AppTabsTrigger>
        </AppTabsList>

        <AppTabsContent value="summary">
          <EntityMetadata
            items={[
              { label: t('metadata.code'), value: warehouse.code ?? '—' },
              { label: t('metadata.address'), value: warehouse.address ?? '—' },
              { label: t('metadata.status'), value: String(warehouse.status) },
            ]}
          />
        </AppTabsContent>

        <AppTabsContent value="stock">
          <p className="text-muted-foreground mb-3 text-sm">{t('stockScopeNote')}</p>
          {warehouseRecords.length === 0 ? (
            <AppEmpty description={t('noLocalStock')} />
          ) : (
            <div className="rounded-md border">
              <AppTable>
                <AppTableHeader>
                  <AppTableRow>
                    <AppTableHead>{t('stockTable.inventoryId')}</AppTableHead>
                    <AppTableHead>{t('stockTable.productId')}</AppTableHead>
                    <AppTableHead>{t('stockTable.variationId')}</AppTableHead>
                    <AppTableHead>{t('stockTable.quantity')}</AppTableHead>
                    <AppTableHead />
                  </AppTableRow>
                </AppTableHeader>
                <AppTableBody>
                  {warehouseRecords.map((record) => (
                    <AppTableRow key={record.id}>
                      <AppTableCell className="font-mono text-xs">{record.id}</AppTableCell>
                      <AppTableCell className="font-mono text-xs">{record.productId}</AppTableCell>
                      <AppTableCell className="font-mono text-xs">
                        {record.productVariationId}
                      </AppTableCell>
                      <AppTableCell>
                        <StockQuantity value={record.quantity} />
                      </AppTableCell>
                      <AppTableCell>
                        <IconButton
                          aria-label={t('actions.view')}
                          onClick={() => router.push(`/inventory/${record.id}`)}
                        >
                          <Eye />
                        </IconButton>
                      </AppTableCell>
                    </AppTableRow>
                  ))}
                </AppTableBody>
              </AppTable>
            </div>
          )}
        </AppTabsContent>

        <AppTabsContent value="transactions">
          <p className="text-muted-foreground mb-3 text-sm">{t('stockScopeNote')}</p>
          {transactionsLoading ? (
            <AppLoading />
          ) : (
            <TransactionTimeline transactions={transactions} />
          )}
        </AppTabsContent>

        <AppTabsContent value="metadata">
          <EntityMetadata
            items={[
              { label: t('metadata.id'), value: warehouse.id },
              {
                label: t('metadata.createdAt'),
                value: new Date(warehouse.createdAt).toLocaleString(),
              },
              {
                label: t('metadata.updatedAt'),
                value: new Date(warehouse.updatedAt).toLocaleString(),
              },
            ]}
          />
        </AppTabsContent>
      </AppTabs>
    </div>
  );
}
