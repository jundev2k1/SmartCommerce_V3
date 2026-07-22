# UI Component Strategy

**Purpose:** Explain the shadcn/Radix wrapping strategy so business code has a stable, internal-only UI API.

**Scope:** UI primitives and their wrapping. Form-specific components are covered in [forms.md](./forms.md).

**Related documents:** [architecture/folder-structure.md](../architecture/folder-structure.md), [conventions/imports-and-boundaries.md](../conventions/imports-and-boundaries.md), [decisions/0003-ui-wrapper-strategy.md](../decisions/0003-ui-wrapper-strategy.md), [forms.md](./forms.md), [theming.md](./theming.md)

**When to read:** Adding any new UI element, or introducing/upgrading a shadcn component.

**When to ignore:** Pure business-logic work (hooks, services, state) with no UI changes.

---

## The rule

Feature code imports UI only from `@/shared/ui`, never from shadcn's generated output or Radix directly.

```ts
// NOT OK
import { Button } from '@/components/ui/button';
import * as Dialog from '@radix-ui/react-dialog';

// OK
import { AppButton, AppDialog } from '@/shared/ui';
```

## Two layers

1. **Primitives layer** (`src/components/ui/*`, shadcn CLI output) — treated as vendored/generated code. Regenerated or upgraded via the shadcn CLI; never hand-edited for app-specific behavior.
2. **`shared/ui/*`** — thin wrappers around the primitives layer that: apply app-wide default props/variants, add app-specific behaviors (e.g. a `Button` with a built-in loading spinner state), and re-export under a single barrel (`shared/ui/index.ts`).

Full rationale in [decisions/0003-ui-wrapper-strategy.md](../decisions/0003-ui-wrapper-strategy.md) — in short, this isolates the codebase from shadcn/Radix API churn and gives one place to add cross-cutting behavior instead of touching every call site.

## Adding a new component

1. `npx shadcn add <component>` → lands in the primitives layer.
2. Create a matching file in `shared/ui/` that imports the primitive, applies any app defaults, and exports it.
3. Add the export to `shared/ui/index.ts`.
4. Feature code imports from `@/shared/ui` only.

If a component needs no app-specific behavior yet, the wrapper can be a pure re-export — the point is the import boundary, not that every wrapper must add logic.

## Wrapper naming

Every `shared/ui` component is `App`-prefixed (see [conventions/naming.md](../conventions/naming.md#app-prefixed-wrapper-components)). The Phase 1 baseline set:

`AppButton, AppInput, AppTextarea, AppSelect, AppCheckbox, AppRadio, AppSwitch, AppModal, AppDialog, AppDrawer, AppTooltip, AppPopover, AppBadge, AppAvatar, AppTable, AppDataTable, AppPagination, AppLoading, AppSkeleton, AppEmpty, AppCard, AppTabs, AppBreadcrumb, AppDropdown, AppSearchBox, AppToaster`

`AppModal` vs `AppDialog`: `AppDialog` mirrors the compound Radix API (`Root`/`Trigger`/`Content`/`Header`/`Footer`) for custom layouts; `AppModal` is a single-component convenience wrapper (`open`, `onOpenChange`, `title`, `footer`, `children`) built on `AppDialog`, for the common create/edit-dialog case that doesn't need a custom layout.

`AppDataTable` is the one wrapper with real logic: it takes `columns`, `data`, `pageCount`, `onPaginationChange`, an optional `toolbar` slot, and a `rowActions` render prop, and builds on TanStack Table's manual-pagination mode plus `AppTable`/`AppPagination` internally. Features configure it; they never touch `useReactTable` directly. See [state/query-strategy.md](../state/query-strategy.md) for the pagination contract it expects from a feature's query hook.

Since Phase 3 (Catalog), `AppDataTable` also supports two additive, backward-compatible capabilities: **row selection** (`enableRowSelection`, controlled `rowSelection`/`onRowSelectionChange`, plus `getRowId` so selection keys match entity ids rather than row index — powers `shared/entity`'s `SelectionPanel`) and an internal **column-visibility** toggle (a "Columns" dropdown, uncontrolled, no new props required). Neither changes behavior for a call site that doesn't opt in.

## `shared/entity` — admin-CRUD-screen scaffolding

A third component category alongside `shared/ui` (third-party wrappers) and `shared/layout` (shell chrome): reusable pieces for building list/detail screens, promoted out of Catalog once four modules needed the same shapes. See [decisions/0014-shared-entity-component-layer.md](../decisions/0014-shared-entity-component-layer.md) for the full rationale.

| Component                               | Purpose                                                                                                                                                                                                                                                                                                                             |
| --------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `EntityHeader`                          | Title + description + actions row atop every list/detail page                                                                                                                                                                                                                                                                       |
| `EntityDetailHeader`                    | Breadcrumb + `EntityHeader`, paired — extracted Phase 8 once five detail pages repeated the identical composition                                                                                                                                                                                                                   |
| `EntityToolbar`                         | Composes `AppSearchBox` + a filters slot + a selection-bar slot + actions                                                                                                                                                                                                                                                           |
| `EntityStatusBadge`                     | Status string/number → `AppBadge`, given an optional label map (handles both free-text statuses like Product's and any future numeric enum)                                                                                                                                                                                         |
| `EntityMetadata`                        | Key/value grid for detail-page metadata (id, createdAt, updatedAt, ...)                                                                                                                                                                                                                                                             |
| `ConfirmDeleteDialog`                   | Generic delete confirmation — never re-implemented per feature                                                                                                                                                                                                                                                                      |
| `FilterPanel`                           | Popover wrapper around arbitrary filter form content                                                                                                                                                                                                                                                                                |
| `SelectionPanel`                        | "N selected" bar + Clear + a bulk-actions slot, shown when `AppDataTable`'s row selection is non-empty — bulk actions render disabled/TODO by default since no backend service in this project exposes a bulk endpoint                                                                                                              |
| `AuditTrailButton` / `AuditTrailDialog` | Self-contained (button) and headless (dialog) audit-trail viewers — `{ service, entityId }` in, a filtered list of matching `ListAuditLogs` entries out. Reused by every entity type; see [backend/audit/README.md](../backend/audit/README.md) for the "no server-side entity-id filter" limitation this works around client-side. |
| `ImageUrlListField`                     | Add/remove/reorder a list of image URL strings with preview — **not a file uploader**; no backend service exposes an upload endpoint, so this manages the `string[]` URL field directly                                                                                                                                             |

## `shared/commerce` — customer-facing commerce components

A fourth component category, added in Phase 4 (Client Mock): reusable pieces for customer-facing browse/cart/checkout/order UI, distinct from `shared/entity`'s admin-CRUD focus. See [decisions/0015-shared-commerce-component-layer.md](../decisions/0015-shared-commerce-component-layer.md).

| Component          | Purpose                                                                                                                                                                                                                         |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `ProductPrice`     | Renders a price as a plain 2-decimal number — neither the Product nor Order contract has a currency field, so no symbol is invented                                                                                             |
| `ProductGrid`      | Generic responsive card-grid layout, no fetching/pagination opinions                                                                                                                                                            |
| `ProductCard`      | Thumbnail/name/price/tags card with a `href` and an optional `actions` slot (rendered outside the link) — shared by `features/shop` (customer) and `features/product-search` (admin), differing only in link target and actions |
| `VariantSelector`  | Picks a product variation; hides `"Inactive"`/`"Discontinued"` ones as a customer-facing presentation choice (not an invented backend rule)                                                                                     |
| `QuantitySelector` | +/- quantity stepper                                                                                                                                                                                                            |
| `CartSummary`      | Totals-only sidebar box (item count, subtotal, an action slot) for the Cart page                                                                                                                                                |
| `CheckoutSummary`  | Read-only line-item recap (no edit controls) for the Checkout page                                                                                                                                                              |
| `OrderStatusBadge` | Renders `OrderStatus`'s raw numeric value — the enum has no published name mapping (see [backend/order/README.md](../backend/order/README.md)), so no Pending/Confirmed/etc. label is guessed                                   |
| `OrderTimeline`    | Literal placeholder — `GetOrderResponse` has no status-history array, so only the current status is shown alongside a note that history isn't exposed                                                                           |

## `shared/inventory` — inventory-domain components

A fifth component category, added in Phase 5 (Inventory): reusable pieces for Warehouses/Stock/Stock Transactions, deliberately independent from Catalog (references product/variation data, never duplicates it). See [decisions/0016-shared-inventory-component-layer.md](../decisions/0016-shared-inventory-component-layer.md).

| Component              | Purpose                                                                                                                                                                                          |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `WarehouseSelector`    | Prop-driven warehouse picker — takes `warehouses` from `features/inventory`'s `useLocalWarehousesQuery`, doesn't fetch itself                                                                    |
| `InventorySummaryCard` | Read-only product/variant/warehouse/quantity summary — shows only a name/SKU, never full Catalog data                                                                                            |
| `StockStatusBadge`     | Renders an opaque numeric code (used for both `WarehouseStatus` and `InventoryTransactionType` — neither has a published mapping) as a single neutral badge variant, never a guessed color/label |
| `TransactionTimeline`  | Real chronological rendering of `GetInventoryHistory`'s transactions — unlike `OrderTimeline`, this isn't a placeholder                                                                          |
| `InventoryToolbar`     | `EntityToolbar` plus a baked-in "open by id" input — the primary navigation method in a module with no list endpoints                                                                            |
| `InventoryFilters`     | Warehouse filter + optional transaction-type filter (omit `typeOptions` to hide the type control)                                                                                                |
| `StockQuantity`        | Renders a quantity plus a derived "In stock"/"Out of stock" badge (from `quantity > 0`, not an invented backend status)                                                                          |

## `shared/notifications` — notification-feed presentation

A sixth component category, added in Phase 7 (Notification Center): reusable, prop-driven pieces for the notification bell/dropdown/detail — no internal data fetching, same rule as `shared/commerce`/`shared/inventory`. See [decisions/0017-shared-notifications-component-layer.md](../decisions/0017-shared-notifications-component-layer.md).

| Component                | Purpose                                                                                                                               |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------- |
| `UnreadBadge`            | Small count/dot badge — renders nothing at 0, "9+" beyond 9                                                                           |
| `NotificationTimestamp`  | Relative time ("5m ago") from a real `createdAt`, refreshed periodically via state (not `Date.now()` called during render)            |
| `NotificationIcon`       | Maps `category`/`type` free-text strings to an icon, generic bell fallback — safe because these are plain strings, not an opaque enum |
| `NotificationSkeleton`   | Loading placeholder rows                                                                                                              |
| `NotificationEmptyState` | Empty/error variants with an optional retry action                                                                                    |
| `NotificationItem`       | One feed row — read/unread is a prop, not derived internally (see `shared/stores/notifications.store.ts`)                             |
| `NotificationList`       | Feed body — loading/error/empty states, optional category grouping, `IntersectionObserver`-driven "load more"                         |
| `NotificationActions`    | "Mark all as read" action row                                                                                                         |

`shared/hooks/useCursorList.ts` (Phase 7) is the generic `useInfiniteQuery` wrapper [decisions/0010](../decisions/0010-pagination-and-cursor-strategy.md) called for — reusable by any future feed, not notification-specific.

## Button hierarchy

`AppButton` is the base (variant/size/loading/icon props, wrapping shadcn's `Button`). Business pages never style `AppButton` by hand — they use one of its semantic presets, each a thin composition fixing a variant/icon/behavior:

| Component         | Preset                                      | Notes                                                                                       |
| ----------------- | ------------------------------------------- | ------------------------------------------------------------------------------------------- |
| `PrimaryButton`   | `variant="default"`                         | Default call-to-action                                                                      |
| `SecondaryButton` | `variant="secondary"`                       |                                                                                             |
| `CreateButton`    | Primary + `Plus` icon                       | Default label from the `common` i18n namespace                                              |
| `DeleteButton`    | `variant="destructive"` + `Trash` icon      | Confirmation is the caller's responsibility (wrap in `AppModal`), not baked into the button |
| `CancelButton`    | `variant="ghost"`                           |                                                                                             |
| `BackButton`      | `variant="ghost"` + `ArrowLeft` icon        | Calls `router.back()` internally                                                            |
| `RefreshButton`   | Icon button, spins its icon while `loading` |                                                                                             |
| `SubmitButton`    | `type="submit"`                             | Reads `formState.isSubmitting` when rendered inside `shared/forms`'s `<Form>`               |
| `IconButton`      | `size="icon"`, `variant="ghost"`            | Generic icon-only action button                                                             |

Rationale: with 15+ modules each needing create/edit/delete actions, a shared semantic button set keeps every module's action bar visually and behaviorally identical without each feature re-deriving variant/icon choices.

## Skeletons / loading states

Loading placeholders used by route-level `loading.tsx` files (see [routing.md](./routing.md)) live in `shared/ui` as well (`AppSkeleton`, plus table/card skeleton variants built from it), so every feature's loading UI stays visually consistent.
