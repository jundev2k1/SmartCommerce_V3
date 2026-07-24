'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  getWarehouse,
  createWarehouse,
  getInventory,
  getInventoryHistory,
  getProductStock,
  searchWarehouses,
  searchInventories,
  searchInventoryTransactions,
  stockIn,
  stockOut,
  adjustStock,
  type GetProductStockParams,
} from '@/services/inventory';
import type { CriteriaFilter } from '@/services/shared/criteria-request';
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
  searches: () => [...warehouseKeys.all, 'search'] as const,
  search: (params: WarehousesSearchParams) => [...warehouseKeys.searches(), params] as const,
};

export const inventoryKeys = {
  all: ['inventory'] as const,
  details: () => [...inventoryKeys.all, 'detail'] as const,
  detail: (id: string) => [...inventoryKeys.details(), id] as const,
  history: (id: string) => [...inventoryKeys.all, 'history', id] as const,
  productStock: (productId: string, params: GetProductStockParams) =>
    [...inventoryKeys.all, 'product-stock', productId, params] as const,
  searches: () => [...inventoryKeys.all, 'search'] as const,
  search: (params: InventoriesSearchParams) => [...inventoryKeys.searches(), params] as const,
  transactionSearches: () => [...inventoryKeys.all, 'transaction-search'] as const,
  transactionSearch: (params: InventoryTransactionsSearchParams) =>
    [...inventoryKeys.transactionSearches(), params] as const,
};

export function useWarehouseQuery(warehouseId: string) {
  return useQuery({
    queryKey: warehouseKeys.detail(warehouseId),
    queryFn: () => getWarehouse(warehouseId),
    enabled: Boolean(warehouseId),
  });
}

export interface WarehousesSearchParams {
  search?: string;
  page?: number;
  pageSize?: number;
}

export function useWarehousesSearchQuery(params: WarehousesSearchParams = {}) {
  return useQuery({
    queryKey: warehouseKeys.search(params),
    queryFn: () =>
      searchWarehouses({
        keyword: params.search || undefined,
        page: params.page,
        pageSize: params.pageSize,
      }),
  });
}

export function useCreateWarehouseMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (values: CreateWarehouseFormValues) => createWarehouse(values),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: warehouseKeys.searches() }),
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

export interface InventoriesSearchParams {
  warehouseId?: string;
  page?: number;
  pageSize?: number;
}

export function useInventoriesSearchQuery(params: InventoriesSearchParams = {}) {
  return useQuery({
    queryKey: inventoryKeys.search(params),
    queryFn: () => {
      const filters: CriteriaFilter[] = [];
      if (params.warehouseId) {
        filters.push({ field: 'warehouseId', operator: 'eq', value: params.warehouseId });
      }
      return searchInventories({ filters, page: params.page, pageSize: params.pageSize });
    },
  });
}

export interface InventoryTransactionsSearchParams {
  warehouseId?: string;
  /** Opaque numeric InventoryTransactionType as a string (see types/inventory-transaction-type.ts). */
  type?: string;
  page?: number;
  pageSize?: number;
}

export function useInventoryTransactionsSearchQuery(
  params: InventoryTransactionsSearchParams = {},
) {
  return useQuery({
    queryKey: inventoryKeys.transactionSearch(params),
    queryFn: () => {
      const filters: CriteriaFilter[] = [];
      if (params.warehouseId) {
        filters.push({ field: 'warehouseId', operator: 'eq', value: params.warehouseId });
      }
      if (params.type) {
        filters.push({ field: 'type', operator: 'eq', value: Number(params.type) });
      }
      return searchInventoryTransactions({ filters, page: params.page, pageSize: params.pageSize });
    },
  });
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
  queryClient.invalidateQueries({ queryKey: inventoryKeys.searches() });
  queryClient.invalidateQueries({ queryKey: inventoryKeys.transactionSearches() });
}

export function useStockInMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ inventoryId, values }: { inventoryId: string; values: StockInFormValues }) =>
      stockIn(inventoryId, { quantity: Number(values.quantity), reason: values.reason }),
    onSuccess: (_data, { inventoryId }) => invalidateInventory(queryClient, inventoryId),
  });
}

export function useStockOutMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ inventoryId, values }: { inventoryId: string; values: StockOutFormValues }) =>
      stockOut(inventoryId, { quantity: Number(values.quantity), reason: values.reason }),
    onSuccess: (_data, { inventoryId }) => invalidateInventory(queryClient, inventoryId),
  });
}

export function useAdjustStockMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ inventoryId, values }: { inventoryId: string; values: AdjustStockFormValues }) =>
      adjustStock(inventoryId, { newQuantity: Number(values.newQuantity), reason: values.reason }),
    onSuccess: (_data, { inventoryId }) => invalidateInventory(queryClient, inventoryId),
  });
}
