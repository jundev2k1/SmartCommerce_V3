/**
 * Registry of business operations that require an Idempotency-Key, per the
 * backend's opt-in `.RequireIdempotency()` middleware (see
 * docs/services/api-layer.md). To add a new one: add an entry here, then pass
 * `{ idempotency: { operationId: IdempotencyOperation.X } }` as the request
 * config on the matching `apiClient` call in `src/services/<service>/`. The
 * Axios interceptor in `shared/lib/api/client.ts` never needs to change.
 */
export const IdempotencyOperation = {
  CreateUser: 'user.create',
  CreateProduct: 'product.create',
  CreateProductCategory: 'product.create-category',
  CreateProductTag: 'product.create-tag',
  CreateOrder: 'order.create',
  UpdateOrderOwnerInfo: 'order.update-owner-info',
} as const;

export type IdempotencyOperationId =
  (typeof IdempotencyOperation)[keyof typeof IdempotencyOperation];
