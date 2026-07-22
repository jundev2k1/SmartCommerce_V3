/**
 * Generic query-key factory matching the pattern in docs/state/query-strategy.md.
 * Usage: const productKeys = createQueryKeys('products'); productKeys.list(filters)
 */
export function createQueryKeys(scope: string) {
  const all = [scope] as const;
  return {
    all,
    lists: () => [...all, 'list'] as const,
    list: (filters: unknown) => [...all, 'list', filters] as const,
    details: () => [...all, 'detail'] as const,
    detail: (id: string) => [...all, 'detail', id] as const,
  };
}
