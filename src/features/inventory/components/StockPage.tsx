'use client';

import { useMemo, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import type { ColumnDef } from '@tanstack/react-table';
import {
  AppDataTable,
  AppEmpty,
  AppLoading,
  AppSearchBox,
  AppSelect,
  AppCard,
  AppCardHeader,
  AppCardTitle,
  AppCardContent,
  IconButton,
} from '@/shared/ui';
import { EntityHeader } from '@/shared/entity';
import { InventoryToolbar, InventoryFilters, StockQuantity } from '@/shared/inventory';
import { Eye } from 'lucide-react';
import { useSearchProductsQuery } from '@/features/products';
import type { GetInventoryResponse } from '@/services/inventory';
import { useProductQuery } from '@/features/products';
import {
  useLocalInventoryQuery,
  useLocalWarehousesQuery,
  useProductStockQuery,
} from '../api/inventory.queries';

const PAGE_SIZE = 20;

export function StockPage() {
  const t = useTranslations('inventory.stock');
  const router = useRouter();

  // --- Section 1: look up a real stock rollup for a product/variation ---
  const [productSearch, setProductSearch] = useState('');
  const [selectedProductId, setSelectedProductId] = useState<string | undefined>(undefined);
  const [selectedVariationId, setSelectedVariationId] = useState<string | undefined>(undefined);

  const { data: productResults } = useSearchProductsQuery({
    search: productSearch,
    page: 1,
    pageSize: 5,
  });
  const { data: selectedProduct } = useProductQuery(selectedProductId ?? '');
  const { data: stock, isLoading: stockLoading } = useProductStockQuery(selectedProductId ?? '', {
    productVariationId: selectedVariationId,
  });

  const variationOptions = (selectedProduct?.variations ?? []).map((v) => ({
    value: v.id,
    label: v.sku ?? v.id,
  }));

  // --- Section 2: locally-known inventory records ---
  const { records, isLoading: recordsLoading, hasAny } = useLocalInventoryQuery();
  const { warehouses } = useLocalWarehousesQuery();
  const warehouseOptions = warehouses.map((w) => ({ id: w.id, label: w.name ?? w.code ?? w.id }));

  const [search, setSearch] = useState('');
  const [filters, setFilters] = useState<{ warehouseId?: string }>({});
  const [page, setPage] = useState(1);

  const filtered = useMemo(() => {
    let result = records;
    if (filters.warehouseId) {
      result = result.filter((r) => r.warehouseId === filters.warehouseId);
    }
    if (search.trim()) {
      const query = search.trim().toLowerCase();
      result = result.filter(
        (r) =>
          r.id.toLowerCase().includes(query) ||
          r.productId.toLowerCase().includes(query) ||
          r.productVariationId.toLowerCase().includes(query),
      );
    }
    return result;
  }, [records, search, filters]);

  const pageCount = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const pageItems = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  const columns: ColumnDef<GetInventoryResponse>[] = [
    { accessorKey: 'id', header: t('table.inventoryId') },
    { accessorKey: 'productId', header: t('table.productId') },
    { accessorKey: 'productVariationId', header: t('table.variationId') },
    { accessorKey: 'warehouseId', header: t('table.warehouseId') },
    {
      id: 'quantity',
      header: t('table.quantity'),
      cell: ({ row }) => <StockQuantity value={row.original.quantity} />,
    },
  ];

  return (
    <div className="space-y-6">
      <EntityHeader title={t('title')} description={t('description')} />

      <AppCard>
        <AppCardHeader>
          <AppCardTitle>{t('lookup.title')}</AppCardTitle>
        </AppCardHeader>
        <AppCardContent className="space-y-3">
          <AppSearchBox
            onValueChange={setProductSearch}
            placeholder={t('lookup.searchPlaceholder')}
            className="max-w-sm"
          />
          <div className="flex flex-wrap gap-2">
            {(productResults?.items ?? []).map((product) => (
              <button
                key={product.id}
                type="button"
                onClick={() => {
                  setSelectedProductId(product.id);
                  setSelectedVariationId(undefined);
                }}
                className="hover:bg-accent data-[selected=true]:border-primary rounded-md border px-3 py-1.5 text-sm"
                data-selected={selectedProductId === product.id}
              >
                {product.name ?? product.code ?? product.id}
              </button>
            ))}
          </div>

          {selectedProductId ? (
            <div className="space-y-3 border-t pt-3">
              {variationOptions.length > 0 ? (
                <AppSelect
                  options={variationOptions}
                  value={selectedVariationId}
                  onValueChange={setSelectedVariationId}
                  placeholder={t('lookup.allVariations')}
                />
              ) : null}
              {stockLoading ? (
                <AppLoading />
              ) : (
                <div className="flex items-center gap-2">
                  <span className="text-muted-foreground text-sm">
                    {t('lookup.totalQuantity')}:
                  </span>
                  <StockQuantity value={stock?.totalQuantity ?? 0} />
                </div>
              )}
              <p className="text-muted-foreground text-xs">{t('lookup.rollupNote')}</p>
            </div>
          ) : null}
        </AppCardContent>
      </AppCard>

      <div className="space-y-3">
        <h2 className="text-lg font-medium">{t('records.title')}</h2>
        <p className="text-muted-foreground text-sm">{t('records.limitationNote')}</p>

        <InventoryToolbar
          onSearchChange={(value) => {
            setSearch(value);
            setPage(1);
          }}
          searchPlaceholder={t('records.searchPlaceholder')}
          filters={
            <InventoryFilters
              value={filters}
              onChange={(next) => {
                setFilters(next);
                setPage(1);
              }}
              warehouses={warehouseOptions}
            />
          }
          onOpenById={(id) => router.push(`/inventory/${id}`)}
        />

        {!hasAny ? (
          <AppEmpty title={t('records.emptyTitle')} description={t('records.emptyDescription')} />
        ) : (
          <AppDataTable<GetInventoryResponse>
            columns={columns}
            data={pageItems}
            page={page}
            pageCount={pageCount}
            onPageChange={setPage}
            isLoading={recordsLoading}
            rowActions={(record) => (
              <IconButton
                aria-label={t('actions.view')}
                onClick={() => router.push(`/inventory/${record.id}`)}
              >
                <Eye />
              </IconButton>
            )}
          />
        )}
      </div>
    </div>
  );
}
