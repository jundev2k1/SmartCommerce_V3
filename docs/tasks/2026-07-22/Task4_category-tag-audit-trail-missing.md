# Task 4: Category and Tag can't view audit trail — reuse the common component

**Status:** Not started.

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

## Status

Not started. No backend dependency expected (assuming `service` values already cover Category/Tag) — purely a frontend wiring task once the detail-surface question (Option A vs B) is decided.
