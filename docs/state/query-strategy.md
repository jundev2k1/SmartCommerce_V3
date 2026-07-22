# TanStack Query Strategy

**Purpose:** Conventions for query keys, pagination, cursor lists, and mutation/cache-invalidation patterns.

**Scope:** TanStack Query only. Boundary against Zustand is in [overview.md](./overview.md). Axios/service layer is in [services/api-layer.md](../services/api-layer.md).

**Related documents:** [overview.md](./overview.md), [services/api-layer.md](../services/api-layer.md), [decisions/0010-pagination-and-cursor-strategy.md](../decisions/0010-pagination-and-cursor-strategy.md)

**When to read:** Adding any data-fetching or mutation logic in a feature.

**When to ignore:** No server-data interaction involved in the current task.

---

## Where query code lives

Every feature's `api/<feature>.queries.ts` holds: a query-key factory, `useQuery`/`useInfiniteQuery` hooks for reads, and `useMutation` hooks for writes. Components only ever call these hooks — never call `src/services/<service>` functions directly. See [frontend/feature-modules.md](../frontend/feature-modules.md).

## Relationship to `src/services/<service>`

Since Phase 1.5, `queryFn`/`mutationFn` call functions from `src/services/<service>/*` (the backend-service-shaped client layer — see [decisions/0012-backend-service-client-layer.md](../decisions/0012-backend-service-client-layer.md)), not `apiClient` directly. A feature's query hook is the seam that reshapes a backend-shaped response into whatever the feature's UI wants — e.g. mapping `BackendPaginatedResult<T>` (`pageNumber`/`hasNextPage`/...) into the `PaginatedResult<T>` shape `AppDataTable`/`AppPagination` expect (`page`/`pageSize`/`totalCount`). That mapping happens here, once per feature; `src/services/` itself never reshapes backend data to fit a frontend convention.

## Query keys

Centralized per feature as a factory object, not ad-hoc arrays scattered across call sites:

```ts
// features/products/api/products.queries.ts
export const productKeys = {
  all: ['products'] as const,
  lists: () => [...productKeys.all, 'list'] as const,
  list: (filters: ProductFilters) => [...productKeys.lists(), filters] as const,
  detail: (id: string) => [...productKeys.all, 'detail', id] as const,
};
```

## Pagination (page-based, default)

Standard list views use page/pageSize params with TanStack Table's manual pagination mode:

```ts
export function useProductsQuery(params: ProductListParams) {
  return useQuery({
    queryKey: productKeys.list(params),
    queryFn: () => productsService.list(params),
    placeholderData: keepPreviousData,
  });
}
```

`keepPreviousData` avoids layout flicker when paging. Full rationale in [decisions/0010-pagination-and-cursor-strategy.md](../decisions/0010-pagination-and-cursor-strategy.md).

## Cursor-based lists (feeds)

Feed-style UIs — notifications, audit log — use `useInfiniteQuery` via a shared wrapper hook, `shared/hooks/useCursorList.ts` (built Phase 7 for Notifications), so every cursor-paginated feature shares the same "load more" contract instead of reimplementing `getNextPageParam` per feature.

`ListMyUserNotifications` (`GET /user-notifications/me`) has a real opaque cursor token: `CursorPaginatedResult<T>` (`items`/`nextCursor`/`hasMore`), not `BackendPaginatedResult<T>`. `useCursorList` drives `useInfiniteQuery`'s `pageParam` directly with `nextCursor`, stopping once `hasMore` is false.

## Mutations & invalidation

```ts
export function useCreateProductMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: productsService.create,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: productKeys.lists() }),
  });
}
```

Invalidate the narrowest key that covers what changed (`lists()`, not `all`, unless a detail view is also affected) to avoid unnecessary refetches.

## Errors

Query/mutation errors surface as a typed `ApiError` (see [services/error-handling.md](../services/error-handling.md)); components read `isError`/`error` from the hook result and render via `shared/ui` error/toast components — never `try/catch` around a `.service.ts` call directly from a component.
