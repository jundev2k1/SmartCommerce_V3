# 0012 — `src/services/` as a Backend-Service-Shaped Client Layer

**Status:** Accepted

**Date:** 2026-07-20 (Phase 1.5)

## Context

Phase 1.5 integrated the first real backend OpenAPI contracts (audit, auth, inventory, notification, order, product, user). Backend services do not map 1:1 to frontend features — the Product service alone backs Catalog, Product Variants, Categories, Tags, and Product Search — so a typed client per backend service can't simply live inside a single `features/<name>/api/` folder without being duplicated or arbitrarily attached to one feature.

## Decision

Add a new top-level layer, `src/services/<backend-service>/`, sitting between `shared/lib/api` (transport: the Axios instance, interceptors, envelope unwrapping) and `features/` (business/UI). Each backend service gets one folder containing one file per endpoint (named after the Swagger `operationId`), each endpoint file self-contained with its own request/response DTO interfaces (named after the Swagger schema), and a barrel `index.ts`. **Superseded 2026-07-22 — see "Update" below**: DTOs no longer live in separate `requests/`/`responses/` subfolders; each endpoint file declares its own.

## Rationale

- **Backend-shaped, not feature-shaped.** This layer's organizing principle is "what does the backend expose," not "what does the frontend need" — that translation happens one layer up, in each feature's own `api/*.queries.ts` (see [state/query-strategy.md](../state/query-strategy.md)), which calls one or more `src/services/<service>` functions and reshapes the result for its own UI.
- **One place per backend service to update** when that service's contract changes, regardless of how many frontend features consume it.
- **Keeps `features/` honest per ADR 0001** — feature folders stay UI/business-flow shaped; they don't accumulate raw endpoint-calling code that would blur the Feature-First boundary.

## Alternatives considered

- **Put endpoint calls directly inside each feature's `api/` folder**: rejected — Product service endpoints would end up duplicated (or awkwardly shared via cross-feature imports) across `features/products`, `features/product-variants`, `features/categories`, `features/tags`, and `features/product-search`.
- **One giant `shared/lib/api/endpoints.ts` file per service**: rejected — directly contradicts the explicit "one API endpoint = one file, do not create giant service files" requirement; also harder to grep/navigate at ~25 endpoints for Product alone.

## Consequences

- Feature query hooks (Phase 2 onward) import from `@/services/<service>` as their `queryFn`/`mutationFn` source — never call `apiClient` directly, and never reach into another service's endpoint files from outside that service (always through the barrel).
- `src/services/shared/paginated-result.ts` holds the one `BackendPaginatedResult<T>` shape reused by every paginated list endpoint (audit, product, notification) — kept separate from the frontend-facing `PaginatedResult<T>` in `shared/types` (page-based, `AppDataTable`-shaped); the query-hook layer maps between the two, not this layer.
- DTO and endpoint names mirror backend terminology (Swagger `operationId`/schema names) verbatim rather than being renamed to a frontend vocabulary — minimizes translation drift as contracts evolve.

## Update (2026-07-22 — dropped `requests/`/`responses/` subfolders)

`src/services/auth/` never actually had `requests/`/`responses/` subfolders — `login.ts`/`register.ts` always declared `LoginRequest`/`RegisterRequest`/`RegisterResponse` inline, an inconsistency with the other six services that went unnoticed until this pass. Rather than retrofit `auth` to match the other six, every service was migrated to match `auth`: each endpoint file now declares its own request/response interface(s) directly, right above the function that uses them. This is a strictly one-file-per-endpoint layer now — no DTO lives outside the endpoint file whose function returns/accepts it.

**Where a DTO is genuinely shared by more than one endpoint** (e.g. `CartResponse` returned by every `order` cart mutation, not just `GET /cart`; `CreateOrderResponse` shared by `POST /orders` and `POST /orders/admin`; `ProductVariationResponse` used by both `GetProduct` and `AddVariation`), it's declared once in whichever endpoint file most naturally owns it — typically the "primary" read endpoint (`GET /cart`, `GET /products/{id}`) or the endpoint the others logically derive from (`POST /orders` for the admin variant) — and every other file imports it type-only from that sibling file (`import type { CartResponse } from './get-cart'`), never re-declared. This never crosses a service boundary; it's always a same-folder sibling import.

**Opaque enum-like domain vocabulary is unaffected** — `types/order-status.ts`, `types/user-status.ts`, `types/warehouse-status.ts`, `types/inventory-transaction-type.ts`, `types/app-role.ts`, `types/audit-action.ts`, `types/audit-node.ts`, `types/product-category-status.ts`, `types/enums.ts` (notification) all stay in each service's `types/` folder, since these represent shared backend vocabulary (frequently reused across many endpoints, sometimes with no published name mapping) rather than a single endpoint's request/response shape. Only request/response DTOs moved; the `types/` folder convention is untouched.

`docs/services/api-layer.md`'s "Endpoint function contract" example and `docs/conventions/naming.md`'s DTO naming row are unaffected — file location changed, naming/shape conventions didn't.

## Related documents

[architecture/folder-structure.md](../architecture/folder-structure.md), [architecture/overview.md](../architecture/overview.md), [services/api-layer.md](../services/api-layer.md), [state/query-strategy.md](../state/query-strategy.md), [backend/README.md](../backend/README.md)
