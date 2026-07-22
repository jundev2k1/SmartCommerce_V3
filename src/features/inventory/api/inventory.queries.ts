'use client';

import { useMutation, useQuery, useQueryClient, useQueries } from '@tanstack/react-query';
import {
  getWarehouse,
  createWarehouse,
  getInventory,
  getInventoryHistory,
  getProductStock,
  stockIn,
  stockOut,
  adjustStock,
  type GetProductStockParams,
} from '@/services/inventory';
import { useLocalWarehousesStore } from '@/shared/stores/local-warehouses.store';
import { useLocalInventoryIdsStore } from '@/shared/stores/local-inventory-ids.store';
import type {
  CreateWarehouseFormValues,
  StockInFormValues,
  StockOutFormValues,
  AdjustStockFormValues,
} from '../inventory.schema';

export const warehouseKeys = {
  all: ['warehouses'] as const,
  details: () => [...warehouseKeys.all, 'detail'] as const,
  detail: (id: string) => [...warehouseKeys.details(), id] as const,
};

export const inventoryKeys = {
  all: ['inventory'] as const,
  details: () => [...inventoryKeys.all, 'detail'] as const,
  detail: (id: string) => [...inventoryKeys.details(), id] as const,
  history: (id: string) => [...inventoryKeys.all, 'history', id] as const,
  productStock: (productId: string, params: GetProductStockParams) =>
    [...inventoryKeys.all, 'product-stock', productId, params] as const,
};

export function useWarehouseQuery(warehouseId: string) {
  return useQuery({
    queryKey: warehouseKeys.detail(warehouseId),
    queryFn: () => getWarehouse(warehouseId),
    enabled: Boolean(warehouseId),
  });
}

/**
 * No warehouse list endpoint exists (see docs/backend/inventory/README.md) —
 * fetches the real GetWarehouse for each id created from this browser.
 */
export function useLocalWarehousesQuery() {
  const warehouseIds = useLocalWarehousesStore((s) => s.warehouseIds);
  const results = useQueries({
    queries: warehouseIds.map((id) => ({
      queryKey: warehouseKeys.detail(id),
      queryFn: () => getWarehouse(id),
    })),
  });

  return {
    warehouses: results.map((r) => r.data).filter((w) => w !== undefined),
    isLoading: results.some((r) => r.isLoading),
    hasAny: warehouseIds.length > 0,
  };
}

export function useCreateWarehouseMutation() {
  return useMutation({
    mutationFn: (values: CreateWarehouseFormValues) => createWarehouse(values),
    onSuccess: (data) => useLocalWarehousesStore.getState().addWarehouseId(data.warehouseId),
  });
}

export function useInventoryQuery(inventoryId: string) {
  return useQuery({
    queryKey: inventoryKeys.detail(inventoryId),
    queryFn: () => getInventory(inventoryId),
    enabled: Boolean(inventoryId),
  });
}

export function useInventoryHistoryQuery(inventoryId: string) {
  return useQuery({
    queryKey: inventoryKeys.history(inventoryId),
    queryFn: () => getInventoryHistory(inventoryId),
    enabled: Boolean(inventoryId),
  });
}

/**
 * No inventory list/search endpoint exists, and no endpoint ever returns an
 * inventoryId starting from a product/warehouse — every id here came from a
 * manual "open by id" lookup or a stock-in/out/adjust action. Fetches the
 * real GetInventory for each.
 */
export function useLocalInventoryQuery() {
  const inventoryIds = useLocalInventoryIdsStore((s) => s.inventoryIds);
  const results = useQueries({
    queries: inventoryIds.map((id) => ({
      queryKey: inventoryKeys.detail(id),
      queryFn: () => getInventory(id),
    })),
  });

  return {
    records: results.map((r) => r.data).filter((record) => record !== undefined),
    isLoading: results.some((r) => r.isLoading),
    hasAny: inventoryIds.length > 0,
  };
}

export interface InventoryTransactionWithSource {
  id: string;
  type: number;
  quantity: number;
  quantityAfter: number;
  reason: string | null;
  createdAt: string;
  inventoryId: string;
}

/**
 * Merges GetInventoryHistory across a set of inventory ids (e.g. all ids
 * known to this browser, or just the ones belonging to one warehouse) since
 * there's no single "list all transactions" endpoint. Sorted newest-first.
 */
export function useTransactionsForInventoryIds(inventoryIds: string[]) {
  const results = useQueries({
    queries: inventoryIds.map((id) => ({
      queryKey: inventoryKeys.history(id),
      queryFn: () => getInventoryHistory(id),
    })),
  });

  const transactions: InventoryTransactionWithSource[] = results
    .flatMap((r, index) =>
      (r.data?.transactions ?? []).map((tx) => ({ ...tx, inventoryId: inventoryIds[index] })),
    )
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());

  return { transactions, isLoading: results.some((r) => r.isLoading) };
}

export function useProductStockQuery(productId: string, params: GetProductStockParams = {}) {
  return useQuery({
    queryKey: inventoryKeys.productStock(productId, params),
    queryFn: () => getProductStock(productId, params),
    enabled: Boolean(productId),
  });
}

function invalidateInventory(queryClient: ReturnType<typeof useQueryClient>, inventoryId: string) {
  queryClient.invalidateQueries({ queryKey: inventoryKeys.detail(inventoryId) });
  queryClient.invalidateQueries({ queryKey: inventoryKeys.history(inventoryId) });
}

export function useStockInMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ inventoryId, values }: { inventoryId: string; values: StockInFormValues }) =>
      stockIn(inventoryId, { quantity: Number(values.quantity), reason: values.reason }),
    onSuccess: (_data, { inventoryId }) => {
      useLocalInventoryIdsStore.getState().addInventoryId(inventoryId);
      invalidateInventory(queryClient, inventoryId);
    },
  });
}

export function useStockOutMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ inventoryId, values }: { inventoryId: string; values: StockOutFormValues }) =>
      stockOut(inventoryId, { quantity: Number(values.quantity), reason: values.reason }),
    onSuccess: (_data, { inventoryId }) => {
      useLocalInventoryIdsStore.getState().addInventoryId(inventoryId);
      invalidateInventory(queryClient, inventoryId);
    },
  });
}

export function useAdjustStockMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ inventoryId, values }: { inventoryId: string; values: AdjustStockFormValues }) =>
      adjustStock(inventoryId, { newQuantity: Number(values.newQuantity), reason: values.reason }),
    onSuccess: (_data, { inventoryId }) => {
      useLocalInventoryIdsStore.getState().addInventoryId(inventoryId);
      invalidateInventory(queryClient, inventoryId);
    },
  });
}
