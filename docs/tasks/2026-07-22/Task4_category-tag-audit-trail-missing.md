# Task 4: Category and Tag can't view audit trail — reuse the common component

**Status:** Done. Built Option B (detail drawer), verified via typecheck + eslint.

## Ask

Category and Tag entities should have the same audit-trail viewing capability that Product/Order/User/Warehouse/Inventory already have, using the shared component.

## Current state

`src/shared/entity/AuditTrailButton.tsx:9-27` and `src/shared/entity/AuditTrailDialog.tsx:44` are already fully generic — props are just `{ service: string; entityId: string }` (plus dialog open-state props). `AuditTrailDialog.tsx`'s own docstring (:38-42) states it's "reused by every entity type (Users, Products, Categories, Tags, ...)" — the intent was always for Categories/Tags to use it too.

**Already wired** in: `ProductDetailPage.tsx:41` (`<AuditTrailButton service="Product" entityId={product.id} />`), `OrderDetailPage.tsx:90` (`service="Order"`), `UsersPage.tsx`, `WarehouseDetailPage.tsx`, `InventoryDetailPage.tsx`.

**Missing entirely** for Categories/Tags — a full grep across both features returns zero matches for `AuditTrail`:

- `src/features/categories/components/CategoriesPage.tsx:65-72` — header (`EntityHeader`) only has a `CreateButton` action. There's no separate category detail page at all — categories are managed inline via `CategoryTree`/`CategoryFormDialog`.
- `src/features/tags/components/TagsPage.tsx:57-64` — same shape: `EntityHeader` actions only contain `CreateButton`, no detail page, no audit wiring.

## Why this isn't a one-line change

`AuditTrailButton` itself needs no changes (already generic, no Product-specific assumptions). The gap is that **Categories and Tags have no detail page to put the button on** — unlike Product/Order/Warehouse/Inventory, which all have a dedicated detail route this button slots into. Category/Tag management happens inline in a tree/list, so this needs a decision on _where_ the button goes:

- **Option A:** add it inline per-row in `CategoryTree`/the tags list (e.g. next to edit/delete actions).
- **Option B:** build a lightweight detail view/drawer for a single category or tag and put the button there, consistent with every other entity.

## Before implementing

Confirm the backend's audit log `service` field actually includes `"Category"` and `"Tag"` as valid values (check `docs/backend/audit/README.md` or the backend's `AuditAction`/service enum) — `AuditTrailDialog` filters by whatever `service` string is passed in, so a mismatched string would silently show an empty list (same failure mode as Task 1).

## Implementation

Chose **Option B** — a lightweight detail drawer per entity, using the existing `AppDrawer` (Sheet) primitive that was already built but unused anywhere in the codebase.

1. **`CategoryDetailDrawer.tsx`** (`src/features/categories/components/`): shows code, status, id, and the `AuditTrailButton`. Opened via a new "view" (`Eye` icon) action threaded through `CategoryTreeNode` → `CategoryTree` → `CategoriesPage`.
2. **`TagDetailDrawer.tsx`** (`src/features/tags/components/`): shows code, id, and the `AuditTrailButton`. Opened via a new "view" action added to `TagsPage`'s row actions.
3. **i18n:** added `detailDrawer` sections to `categories.json` and `tags.json`.

### Resolved the "before implementing" caution — service name, not entity subtype

Checked how every other `AuditTrailButton` call site works (`ProductDetailPage.tsx`: `service="Product"`, `OrderDetailPage.tsx`: `service="Order"`) — `service` is always the **originating backend service name**, never the entity subtype. Per `docs/backend/product/README.md`, Category and Tag are owned by the Product service ("Catalog (Products, Variants, Categories, Tags, Search)"). So both drawers pass `service="Product"` (not `"Category"`/`"Tag"`) — the entity subtype is presumably distinguished by `rootEntityType` in the audit log, which `AuditTrailDialog` doesn't filter on anyway (it only matches `service` + `rootEntityId`). Passing `"Category"`/`"Tag"` as the service would have silently reproduced Task 1's exact failure mode (200 response, empty render). This is still an assumption pending backend confirmation — flagged with a code comment at both call sites — but it follows the codebase's own established convention rather than guessing a new one.

Verified: `tsc --noEmit` and `eslint` both clean.
