# Backend: Product Service

**Purpose:** What the Product Service exposes and how `src/services/product` maps to it.

**Scope:** This service's contract only.

**Related documents:** [backend/README.md](../README.md), [backend/feature-mapping.md](../feature-mapping.md), [backend/coverage-report.md](../coverage-report.md), `src/services/product/`

**When to read:** Building Catalog (Products, Variants, Categories, Tags, Search — Phases 5–6). The biggest single backend service in this integration (25 endpoints).

**When to ignore:** Any work not touching the product catalog.

---

## Purpose

Full product catalog: products (with variations created as an aggregate), categories (hierarchical), tags (flat), and Elasticsearch-backed search — plus an index-rebuild admin operation.

## Base path

`/api/product`.

## Auth requirements

Not explicitly documented per-endpoint. `RebuildProductSearchIndex` is presumably admin-only in practice (destructive/expensive operation) though the contract doesn't say so explicitly.

## Endpoints (25)

| Area         | Operations                                                                                                                                                                                                                                                                      |
| ------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Products     | `CreateProduct` (aggregate init w/ variations), `GetProduct`, `UpdateProduct` (shared fields only, never variations), `DeleteProduct` (hard delete, cascades variations), `SearchProducts` (Elasticsearch, paginated)                                                           |
| Variations   | `AddVariation`, `UpdateVariation` (never touches displayOrder/isDefault), `DeleteVariation` (last one can't be removed), `ReorderVariations`, `SetDefaultVariation`                                                                                                             |
| Categories   | `CreateProductCategory`, `GetProductCategory`, `UpdateProductCategory` (can re-parent), `DeleteProductCategory` (refuses if has children/assigned), `ListProductCategories` (flat, unpaginated), `AssignProductCategory`/`RemoveProductCategory` (product↔category, idempotent) |
| Tags         | `CreateProductTag`, `GetProductTag`, `UpdateProductTag` (rename only), `DeleteProductTag` (refuses if assigned), `ListProductTags` (flat, unpaginated), `AssignProductTag`/`RemoveProductTag` (idempotent)                                                                      |
| Search admin | `RebuildProductSearchIndex`                                                                                                                                                                                                                                                     |

See `src/services/product/index.ts` for the full function list — one file per operation, matching this table.

## Request/response DTOs

`CreateProductRequest` embeds `CreateProductVariationRequest[]` for aggregate creation; `VariationInput` is a shared base type between `AddVariationRequest` and `CreateProductVariationRequest` (both have identical sku/price/barcode/cost/weight/dimensions/images fields — declared once in `add-variation.ts` and imported type-only by `create-product.ts` rather than duplicated, per [decisions/0012-backend-service-client-layer.md](../../decisions/0012-backend-service-client-layer.md)). Each endpoint file in `src/services/product/` otherwise declares its own request/response DTOs inline; enum-like types live in `.../types/`.

`SearchProductsItemResponse.isInStock` (`boolean | null`) is merged in from a batched Inventory Service gRPC call (`GetProductsStock`) keyed by each item's `defaultVariationId` — not read from Product's own Postgres/Elasticsearch data. `null` means either there's no default variation to check, or Inventory Service was unreachable for that page (fail-open: search itself never fails just because stock can't be confirmed). No quantities are exposed, only the boolean. See [backend/inventory/README.md](../inventory/README.md).

## Pagination style

`SearchProducts` is page-based, wrapped in `BackendPaginatedResult<SearchProductsItemResponse>`. Categories and Tags "list" endpoints are **not paginated** — they return the full flat set (`ListProductCategoriesResponse.categories`, `ListProductTagsResponse.tags`), per their own prose: "not paginated - category/tag counts are small reference data."

## Error responses

Prose-only per endpoint: 400 (validation), 404 (not found / referenced id doesn't exist), 409 (conflict — duplicate code/slug/sku, category has children, tag still assigned, reorder cycle). No formal error schema.

## Known limitations / TODOs

- [ ] `ProductCategoryStatus` (1, 2) has no published name mapping — kept opaque.
- [ ] Variation `status` and `SearchProducts`' `status` filter are both modeled as **plain strings** in the Swagger schema (not an enum $ref), even though the prose lists specific values ("Active"/"Inactive"/"Discontinued"). Typed as `string | null` / `string` rather than a union, since the schema itself doesn't constrain it and inventing a closed union would risk rejecting a legitimate backend value.
- [ ] `RebuildProductSearchIndex` has no documented auth requirement despite being a heavy admin operation — worth confirming before exposing it in any UI.
- [ ] No endpoint returns a single category's or tag's _usage count_ (how many products reference it) — `DeleteProductCategory`/`DeleteProductTag` just refuse with a 409 if in use, so the frontend can't proactively warn "this tag is used by N products" before a delete attempt.
