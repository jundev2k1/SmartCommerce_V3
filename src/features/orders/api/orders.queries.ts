'use client';

import { useMutation, useQuery, useQueryClient, useQueries } from '@tanstack/react-query';
import {
  getOrder,
  cancelOrder,
  completeOrder,
  updateOrderOwnerInfo,
  searchOrders,
  OrderStatus,
} from '@/services/order';
import { useLocalOrdersStore } from '@/shared/stores/local-orders.store';
import type { CriteriaFilter } from '@/services/shared/criteria-request';
import type { UpdateOrderOwnerInfoFormValues } from '../orders.schema';

export const orderKeys = {
  all: ['orders'] as const,
  details: () => [...orderKeys.all, 'detail'] as const,
  detail: (id: string) => [...orderKeys.details(), id] as const,
  searches: () => [...orderKeys.all, 'search'] as const,
  search: (params: OrdersSearchParams) => [...orderKeys.searches(), params] as const,
};

export function useOrderQuery(orderId: string) {
  return useQuery({
    queryKey: orderKeys.detail(orderId),
    queryFn: () => getOrder(orderId),
    enabled: Boolean(orderId),
  });
}

export interface OrdersSearchParams {
  /** Free-text match against customerName — the only keyword-searchable field. */
  keyword?: string;
  status?: OrderStatus;
  phone?: string;
  createdFrom?: string;
  createdTo?: string;
  sortBy?: 'customerName' | 'status' | 'createdAt';
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
}

/**
 * SearchOrders' `status` filter is parsed on the backend via `Enum.Parse`
 * (BuildingBlock.Criteria's Enum field strategy), so the wire value must be
 * the C# enum's string name, not the numeric value this app otherwise uses
 * for OrderStatus everywhere else.
 */
const ORDER_STATUS_FILTER_VALUE: Record<OrderStatus, string> = {
  [OrderStatus.Pending]: 'Pending',
  [OrderStatus.Confirmed]: 'Confirmed',
  [OrderStatus.Cancelled]: 'Cancelled',
  [OrderStatus.Completed]: 'Completed',
};

/** Admin-only — POST /orders/search, the real system-wide order list/search. */
export function useOrdersSearchQuery(params: OrdersSearchParams) {
  return useQuery({
    queryKey: orderKeys.search(params),
    queryFn: () => {
      const filters: CriteriaFilter[] = [];
      if (params.status !== undefined) {
        filters.push({
          field: 'status',
          operator: 'eq',
          value: ORDER_STATUS_FILTER_VALUE[params.status],
        });
      }
      if (params.phone?.trim()) {
        filters.push({ field: 'phone', operator: 'eq', value: params.phone.trim() });
      }
      if (params.createdFrom) {
        filters.push({ field: 'createdAt', operator: 'gte', value: params.createdFrom });
      }
      if (params.createdTo) {
        filters.push({ field: 'createdAt', operator: 'lte', value: params.createdTo });
      }
      return searchOrders({
        keyword: params.keyword || undefined,
        filters,
        sorts: params.sortBy
          ? [{ field: params.sortBy, direction: params.sortDescending ? 'desc' : 'asc' }]
          : undefined,
        page: params.page,
        pageSize: params.pageSize,
      });
    },
  });
}

/**
 * "My Orders" has no backing list endpoint — `SearchOrders` is Admin-only,
 * and no customer-scoped list endpoint exists (see
 * docs/backend/order/README.md) — so it fetches the real GetOrder response
 * for each id tracked in local-orders.store — every field is real backend
 * data, just scoped to orders placed from this browser. See
 * docs/modules/client-mock.md.
 */
export function useLocalOrdersQuery() {
  const orderIds = useLocalOrdersStore((s) => s.orderIds);
  const results = useQueries({
    queries: orderIds.map((id) => ({
      queryKey: orderKeys.detail(id),
      queryFn: () => getOrder(id),
    })),
  });

  return {
    orders: results.map((r) => r.data).filter((order) => order !== undefined),
    isLoading: results.some((r) => r.isLoading),
    hasAny: orderIds.length > 0,
  };
}

export function useCancelOrderMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (orderId: string) => cancelOrder(orderId),
    onSuccess: (_data, orderId) => {
      queryClient.invalidateQueries({ queryKey: orderKeys.detail(orderId) });
      queryClient.invalidateQueries({ queryKey: orderKeys.searches() });
    },
  });
}

/** Admin-only — POST /orders/{orderId}/complete, only valid on a Confirmed order. */
export function useCompleteOrderMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (orderId: string) => completeOrder(orderId),
    onSuccess: (_data, orderId) => {
      queryClient.invalidateQueries({ queryKey: orderKeys.detail(orderId) });
      queryClient.invalidateQueries({ queryKey: orderKeys.searches() });
    },
  });
}

/** PATCH /orders/{orderId}/owner-info — only valid while the order is Pending or Confirmed. */
export function useUpdateOrderOwnerInfoMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      orderId,
      values,
    }: {
      orderId: string;
      values: UpdateOrderOwnerInfoFormValues;
    }) => updateOrderOwnerInfo(orderId, values),
    onSuccess: (_data, { orderId }) =>
      queryClient.invalidateQueries({ queryKey: orderKeys.detail(orderId) }),
  });
}
