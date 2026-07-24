'use client';

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
  useInventoriesSearchQuery,
  useInventoryTransactionsSearchQuery,
} from '../api/inventory.queries';

// Detail tabs show one page of records rather than adding pagination UI to a
// tab view — a warehouse with more than this many rows/transactions only
// shows the first page here (see the Stock / Stock Transactions pages for
// the fully paginated views).
const DETAIL_TAB_PAGE_SIZE = 50;

export function WarehouseDetailPage({ warehouseId }: { warehouseId: string }) {
  const t = useTranslations('inventory.warehouses.detail');
  const tNav = useTranslations('nav');
  const router = useRouter();
  const { data: warehouse, isLoading } = useWarehouseQuery(warehouseId);
  const { data: inventoriesResult } = useInventoriesSearchQuery({
    warehouseId,
    pageSize: DETAIL_TAB_PAGE_SIZE,
  });
  const warehouseRecords = inventoriesResult?.items ?? [];
  const { data: transactionsResult, isLoading: transactionsLoading } =
    useInventoryTransactionsSearchQuery({ warehouseId, pageSize: DETAIL_TAB_PAGE_SIZE });
  const transactions = transactionsResult?.items ?? [];

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
          {warehouseRecords.length === 0 ? (
            <AppEmpty description={t('noStock')} />
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
