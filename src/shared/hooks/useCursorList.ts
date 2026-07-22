'use client';

import { useInfiniteQuery, type QueryKey } from '@tanstack/react-query';
import type { CursorPaginatedResult } from '@/services/shared/paginated-result';

export interface UseCursorListOptions<TItem, TParams> {
  queryKey: QueryKey;
  queryFn: (
    params: TParams & { cursor: string | undefined; limit: number },
  ) => Promise<CursorPaginatedResult<TItem>>;
  params: TParams;
  limit: number;
  enabled?: boolean;
}

/**
 * Shared "load more on scroll" wrapper over `useInfiniteQuery`, for feed-style
 * lists (notifications, and any future append-only feed) per
 * docs/decisions/0010-pagination-and-cursor-strategy.md. Drives
 * `useInfiniteQuery`'s `pageParam` with the backend's real opaque cursor
 * token (`nextCursor`), stopping once `hasMore` is false.
 */
export function useCursorList<
  TItem,
  TParams extends Record<string, unknown> = Record<string, never>,
>({ queryKey, queryFn, params, limit, enabled = true }: UseCursorListOptions<TItem, TParams>) {
  const query = useInfiniteQuery({
    queryKey: [...queryKey, params, limit],
    queryFn: ({ pageParam }) =>
      queryFn({ ...params, cursor: pageParam, limit } as TParams & {
        cursor: string | undefined;
        limit: number;
      }),
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) =>
      lastPage.hasMore ? (lastPage.nextCursor ?? undefined) : undefined,
    enabled,
  });

  const items = query.data?.pages.flatMap((page) => page.items) ?? [];

  return {
    items,
    isLoading: query.isLoading,
    isError: query.isError,
    hasMore: Boolean(query.hasNextPage),
    isFetchingMore: query.isFetchingNextPage,
    loadMore: () => query.fetchNextPage(),
    refetch: query.refetch,
  };
}
