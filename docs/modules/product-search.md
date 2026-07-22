# Product Search

**Purpose:** A card-grid browse/search experience over the catalog, built to be reused by a future customer-facing Client page.

**Scope:** `features/product-search` and the `(admin)/product-search` route.

**Related documents:** [modules/overview.md](./overview.md), [modules/product-management.md](./product-management.md)

**When to read:** Building a customer-facing catalog browse experience, or touching the reusable search/filter pieces this shares with Products.

**When to ignore:** Admin product CRUD — that's [product-management.md](./product-management.md).

---

## Why this exists as a separate module from Products

Both hit the exact same `SearchProducts` endpoint. Rather than duplicate that (or leave "Product Search" as a redundant nav entry pointing at the same table Products already shows), this module demonstrates the "reusable by future Client pages" requirement concretely: it reuses `features/products`'s `useSearchProductsQuery` and `ProductFilters` (via its public barrel — a legitimate cross-feature dependency, not an internal reach-in) but renders results as a **card grid** with no admin actions (no create/edit/delete), closer to what a real customer-facing browse page would look like.

## Feature folder(s)

`src/features/product-search/`

## Key flows

Search (`AppSearchBox`) + filters (`FilterPanel` + `features/products`'s `ProductFilters`) + a paginated card grid (thumbnail, name, price, tag badges) over `useSearchProductsQuery`.

## Dependencies on other modules

Depends on **Product Management** for `useSearchProductsQuery` and `ProductFilters` — intentionally, to avoid a second implementation of catalog search.

## Open questions / backend dependencies

None beyond what's already documented in [backend/product/README.md](../backend/product/README.md) for `SearchProducts` itself.

## Update (Phase 4 — Client Mock)

This page's cards now use `shared/commerce`'s `ProductCard`/`ProductGrid` (linking to `/products/[id]`, the admin detail page) — the same components `features/shop`'s customer-facing browse page uses (linking to `/shop/[id]` instead, with an "Add to cart" action). This is the "reused by a future Client page" prediction this doc made, now concretely true. See [modules/client-mock.md](./client-mock.md) and [decisions/0015-shared-commerce-component-layer.md](../decisions/0015-shared-commerce-component-layer.md).
