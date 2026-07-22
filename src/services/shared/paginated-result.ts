/**
 * Raw backend pagination shape, identical across every paginated list endpoint
 * (audit, product, notification). Distinct from the frontend-facing
 * `PaginatedResult<T>` in `@/shared/types` — mapping between the two happens in
 * each feature's query hook, not here. See docs/state/query-strategy.md.
 */
export interface BackendPaginatedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  totalPages: number;
}

/**
 * Opaque cursor pagination shape, used by list endpoints migrated off
 * page-number pagination (currently `ListMyUserNotifications`). See
 * `BuildingBlock.Application.Abstractions.Common.CursorPaginatedResult` on
 * the backend.
 */
export interface CursorPaginatedResult<T> {
  items: T[];
  nextCursor: string | null;
  hasMore: boolean;
}
