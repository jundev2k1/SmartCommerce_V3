# Product Categories

**Purpose:** Manage the hierarchical category tree products are organized into.

**Scope:** `features/categories` and the `(admin)/categories` route.

**Related documents:** [modules/overview.md](./overview.md), [modules/product-management.md](./product-management.md), [backend/product/README.md](../backend/product/README.md)

**When to read:** Touching category CRUD, the tree UI, or re-parenting behavior.

**When to ignore:** Assigning categories to a specific product — see [product-management.md](./product-management.md)'s Categories tab instead.

---

## Feature folder(s)

`src/features/categories/`

## Data model

`ListProductCategories` returns a **flat list** with `parentCategoryId` per item — its own description says this is deliberate: "so a client can assemble the hierarchy tree itself." `categories.utils.ts`'s `buildCategoryTree` does exactly that, recursively, with no depth assumption or cap. `UpdateProductCategory` already supports re-parenting (the server enforces cycle-prevention with a 409) — that's the "move" operation; no separate endpoint exists or is needed.

## Key flows

- **Tree display**: `CategoriesPage` → `useCategoriesTreeQuery` (wraps `ListProductCategories` + `buildCategoryTree`) → `CategoryTree`/`CategoryTreeNode`, recursive expand/collapse.
- **Search**: client-side, over the already-fetched flat set — `ListProductCategories` is explicitly "not paginated, small reference data," so there's no server-side search to call.
- **Create/Edit**: `CategoryFormDialog`, including a parent picker that excludes the node's own descendants client-side (`collectDescendantIds`) — the server's 409 remains the actual source of truth for cycle prevention; the client-side exclusion is just a better first-pass UX, not a substitute for it.
- **Delete**: refuses server-side (409) if the category has children or is still assigned to a product — surfaced as a normal `ApiError` toast, not a crash.

## Dependencies on other modules

Consumed by **Product Management** (category-assignment checklist, list-page filter) and **Product Search** (filter) via this feature's public barrel.

## Open questions / backend dependencies

- `ProductCategoryStatus` (1, 2) has no published name mapping — not surfaced in the UI at all rather than guessed.
- No usage-count endpoint — see [product-management.md](./product-management.md)'s note.
