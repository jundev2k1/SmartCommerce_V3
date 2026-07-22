import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { BackendPaginatedResult } from '@/services/shared/paginated-result';

export interface SearchProductsParams {
  search?: string;
  categoryId?: string;
  tagId?: string;
  /** Free-text status filter (e.g. "Active"/"Inactive"/"Discontinued"). */
  status?: string;
  sortBy?: 'name' | 'price' | 'updatedAt';
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
}

export interface SearchProductsItemResponse {
  id: string;
  code: string | null;
  name: string | null;
  slug: string | null;
  thumbnail: string | null;
  defaultPrice: number | null;
  defaultVariationId: string | null;
  defaultVariationSku: string | null;
  categoryIds: string[] | null;
  categoryNames: string[] | null;
  tagIds: string[] | null;
  tagNames: string[] | null;
  /** Free-text status (e.g. "Active"/"Inactive"/"Discontinued"). */
  status: string | null;
  updatedAt: string;
}

/** GET /products — paginated, searchable, filterable list, served from Elasticsearch. */
export function searchProducts(
  params: SearchProductsParams = {},
): Promise<BackendPaginatedResult<SearchProductsItemResponse>> {
  return apiClient.get(`${BASE_PATH}/products`, { params }).then((res) => res.data);
}
