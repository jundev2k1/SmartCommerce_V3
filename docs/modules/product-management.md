# Product Management

**Purpose:** Manage the product catalog — products, and the variants that live inside them.

**Scope:** `features/products` and the `(admin)/products` routes (list + `[productId]` detail). Includes what was originally planned as a separate "Product Variant Management" module — see below.

**Related documents:** [modules/overview.md](./overview.md), [backend/product/README.md](../backend/product/README.md), [decisions/0014-shared-entity-component-layer.md](../decisions/0014-shared-entity-component-layer.md)

**When to read:** Touching product or variant CRUD, catalog search/filtering, or category/tag assignment on a product.

**When to ignore:** Category/Tag CRUD themselves (see their own module docs) — this doc only covers assigning them to a product.

---

## Why Variants don't have their own module

The original module list ([modules/overview.md](./overview.md)) planned "Product Variant Management" as a separate nav entry. The real contract makes clear that's wrong: every variation endpoint (`AddVariation`, `UpdateVariation`, `DeleteVariation`, `ReorderVariations`, `SetDefaultVariation`) requires a `productId` — there is no way to list or manage variations independently of a specific product. The nav entry was removed; variants are the "Variants" tab on a product's detail page (`ProductVariantsTab`).

## Feature folder(s)

`src/features/products/`

## Data model (real backend contract — no gaps, unlike Users)

`CreateProductRequest` (aggregate creation — a product plus its initial variation(s) in one call), `UpdateProductRequest` (name/description/slug only, never variation data), `SearchProductsParams`/`SearchProductsItemResponse` (Elasticsearch-backed, paginated), `ProductVariationResponse`. See `src/services/product/`.

## Key flows

- **List**: `ProductsListPage` — `AppDataTable` with real search/filter/sort/pagination/column-visibility/row-selection, all against `SearchProducts`. Row selection is genuinely wired (`enableRowSelection`), but the bulk-actions bar it powers (`SelectionPanel`) renders disabled — no bulk endpoint exists on this or any service (see [backend/coverage-report.md](../backend/coverage-report.md)).
- **Create**: `ProductCreateDialog` collects the minimum (code/name/slug/description + one variation's sku/price) since `CreateProduct` requires at least one variation up front; redirects to the new product's detail page on success.
- **Detail**: `ProductDetailPage` — breadcrumb, `EntityHeader` with an `AuditTrailButton`, and tabs: General (update name/description/slug), Variants, Categories, Tags, Metadata.
- **Variants tab**: add/edit/delete/reorder/set-default, all against real endpoints. Delete is disabled with an explanatory tooltip when only one variation remains (client-side mirror of the server's own "last variation can't be removed" rule — the server still enforces it with a 400 regardless).
- **Categories/Tags tabs**: checklists against the full category tree / tag list (from `features/categories`/`features/tags`'s public barrels), calling `AssignProductCategory`/`RemoveProductCategory` (and the tag equivalents) per toggle.

## Number fields are validated strings, not `z.coerce.number()`

`shared/forms/useAppForm` requires a schema's input and output types to match exactly; `z.coerce.number()`'s input type is `unknown`, which fails that constraint. Price/cost/weight fields are kept as validated numeric strings in the form schema and parsed to real numbers only when the request payload is built (`api/products.queries.ts`) — not a shared-forms change, contained entirely within this feature's own schema/queries files.

## Images: URL list, not upload

Variation `images` is `string[]` on the wire with no upload endpoint anywhere in this project's 7 backend services. `VariantFormDialog` uses `shared/entity`'s `ImageUrlListField` — add/remove/reorder URL strings with a preview — not a file picker.

## Dependencies on other modules

Reads from **Product Categories** and **Product Tags** (via their public barrels) for the assignment checklists and the list-page filters. **Product Search** reads from this module's `useSearchProductsQuery` and `ProductFilters` rather than duplicating them.

## Open questions / backend dependencies

- Product's free-text `status` field (and the `SearchProducts.status` filter) isn't a closed enum in the schema, even though prose names exactly three values ("Active"/"Inactive"/"Discontinued") — kept as a plain string rather than a guessed union, per [backend/product/README.md](../backend/product/README.md).
- No endpoint reports a category's/tag's product-usage count — can't warn "this is used by N products" before a delete attempt; the 409 from the server is the only signal.
