/**
 * Generic admin-search request body shared by every `POST /{resource}/search`
 * endpoint (Users, Warehouses, Inventories, Inventory Transactions — see
 * `BuildingBlock.Criteria.Requests.CriteriaRequest` on the backend). Distinct
 * from `SearchProductsParams`, which is `GET` + query string, not this shape.
 */
export interface CriteriaFilter {
  field: string;
  operator:
    'eq' | 'ne' | 'c' | 'sw' | 'ew' | 'in' | 'nin' | 'gt' | 'gte' | 'lt' | 'lte' | 'between';
  value: unknown;
}

export interface CriteriaSort {
  field: string;
  direction: 'asc' | 'desc';
}

export interface CriteriaRequest {
  keyword?: string;
  filters?: CriteriaFilter[];
  sorts?: CriteriaSort[];
  page?: number;
  pageSize?: number;
}
