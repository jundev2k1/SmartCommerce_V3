# Product Tags

**Purpose:** Manage the flat (non-hierarchical) product tag set.

**Scope:** `features/tags` and the `(admin)/tags` route.

**Related documents:** [modules/overview.md](./overview.md), [modules/product-management.md](./product-management.md), [backend/product/README.md](../backend/product/README.md)

**When to read:** Touching tag CRUD.

**When to ignore:** Assigning tags to a specific product — see [product-management.md](./product-management.md)'s Tags tab instead.

---

## Feature folder(s)

`src/features/tags/`

## Data model

`CreateProductTagRequest{code,name}`, `UpdateProductTagRequest{name}` (rename only). `ListProductTags` is flat and unpaginated ("small reference data," same as categories).

## Two features from the original brief were dropped as unsupported, not faked

- **Color**: no `color` field exists anywhere in the Tag schema (`ProductTagItemResponse`/`GetProductTagResponse` are just `{id, code, name, ...}`). No color swatch is rendered.
- **Usage count**: no endpoint reports how many products reference a tag. `DeleteProductTag` just refuses with a 409 if still assigned — that's the only signal available, surfaced as a normal error toast.

Both are documented here rather than invented, per the same policy applied to the Users-module gaps in Phase 2.

## Key flows

Standard CRUD: `TagsPage` (client-side search + paginated table over the full fetched set, `AppDataTable`), `TagFormDialog` (create/edit), delete via `ConfirmDeleteDialog`.

## Dependencies on other modules

Consumed by **Product Management** (tag-assignment checklist, list-page filter) and **Product Search** (filter).

## Open questions / backend dependencies

If the backend ever adds `color` or a usage-count field/endpoint, this module is the only place that needs updating — the table/form architecture already accommodates new columns/fields without restructuring.
