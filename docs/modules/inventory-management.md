# Inventory Management (Warehouses, Stock, Stock Transactions)

**Purpose:** Warehouse administration, stock lookup, and stock movement (in/out/adjust) — independent from Catalog, referencing products/variations by id only.

**Scope:** `features/inventory` and the `(admin)/warehouses`, `(admin)/inventory`, `(admin)/stock-transactions` routes, plus the `shared/inventory` component layer.

**Related documents:** [modules/overview.md](./overview.md), [backend/inventory/README.md](../backend/inventory/README.md), [decisions/0016-shared-inventory-component-layer.md](../decisions/0016-shared-inventory-component-layer.md), [state/zustand-strategy.md](../state/zustand-strategy.md).

**When to read:** Touching Warehouses/Stock/Stock Transactions.

**When to ignore:** Catalog work (Products/Categories/Tags) — Inventory only references product/variation ids, it never duplicates their data.

---

## Feature folder(s)

`src/features/inventory/`. Shared presentation in `src/shared/inventory/`.

## Routes

`/warehouses`, `/warehouses/[warehouseId]`, `/inventory` (Stock), `/inventory/[inventoryId]`, `/stock-transactions`.

## Contract shape

- **Warehouse**: `CreateWarehouse`, `GetWarehouse`, and (as of the backend's 2026-07-24 search endpoints) `POST /warehouses/search` — paginated, keyword + filter/sort. **Still no Update, no Delete.**
- **Inventory**: `GetInventory(id)`, `GetInventoryHistory(id)`, and `POST /inventories/search` (filterable by productId/productVariationId/warehouseId/quantity/createdAt — no free-text keyword, stock rows have no text field) plus `POST /inventory-transactions/search` (not scoped to one inventory id, unlike `GetInventoryHistory`). `GetProductStock(productId, {productVariationId?})` still only returns a rollup `totalQuantity`, never a per-warehouse breakdown.
- `WarehouseStatus` (1–2) and `InventoryTransactionType` (1–3) both have unpublished, unconfirmed value mappings — the Stock Transactions type filter therefore offers the three raw opaque values, not derived labels.

`WarehousesListPage`/`StockPage`/`StockTransactionsPage`/`WarehouseDetailPage` are now real server-paginated views backed by these search endpoints — the earlier "local-tracking + manual ID lookup" workaround (`local-warehouses.store.ts`/`local-inventory-ids.store.ts`) was retired once they landed. `shared/inventory`'s `InventoryToolbar` still offers an "Open by ID" quick-jump alongside the real search/filter UI.

## What's real vs. placeholder

| Requested (brief)                                                              | Status                  | Why                                                                                                                                                                                                           |
| ------------------------------------------------------------------------------ | ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Warehouse Create                                                               | ✅ Real                 | `CreateWarehouse` exists                                                                                                                                                                                      |
| Warehouse Detail                                                               | ✅ Real                 | `GetWarehouse` exists                                                                                                                                                                                         |
| Warehouse List/Search                                                          | ✅ Real                 | `POST /warehouses/search`, server-paginated                                                                                                                                                                   |
| Warehouse Update                                                               | ❌ Disabled + tooltip   | No endpoint at all                                                                                                                                                                                            |
| Warehouse Delete                                                               | ❌ Disabled + tooltip   | No endpoint at all                                                                                                                                                                                            |
| Stock: Current Stock                                                           | ✅ Real (rollup)        | `GetProductStock`/`GetInventory`                                                                                                                                                                              |
| Stock: Reserved / Available / Safety Stock                                     | ❌ Omitted, not faked   | No such fields anywhere in the contract                                                                                                                                                                       |
| Stock: Warehouse / Product / Variant reference                                 | ✅ Real                 | `GetInventoryResponse` has all three ids                                                                                                                                                                      |
| Stock: Status                                                                  | ❌ Omitted              | `GetInventoryResponse` has no status field (only Warehouse does)                                                                                                                                              |
| Stock In / Out / Adjust                                                        | ✅ Real                 | `StockIn`/`StockOut`/`AdjustStock`                                                                                                                                                                            |
| Stock Transfer                                                                 | ❌ Disabled placeholder | No transfer endpoint anywhere                                                                                                                                                                                 |
| Transaction History (per record)                                               | ✅ Real                 | `GetInventoryHistory`                                                                                                                                                                                         |
| Transaction search/pagination (across records)                                 | ✅ Real                 | `POST /inventory-transactions/search`, server-paginated, filterable by warehouse/type                                                                                                                         |
| Transaction "Operator"                                                         | ❌ Omitted              | Not in `InventoryTransactionDto`                                                                                                                                                                              |
| Transaction "Reference"                                                        | Rendered as **Reason**  | The contract's actual field is a free-text `reason`, not a distinct "reference" concept — labeled honestly rather than invented                                                                               |
| Transaction "Quantity Before"                                                  | ❌ Omitted              | `quantity`/`quantityAfter` have no confirmed sign/delta convention across StockIn/StockOut/Adjust (Adjust's request is an absolute `newQuantity`, not a delta) — computing a "before" value would be guessing |
| Transaction type badges (Stock In/Out/Adjustment/Reservation/Release/Transfer) | ⚠️ Opaque only          | The enum only has 3 unconfirmed values total — "Reservation"/"Release"/"Transfer" don't correspond to anything in this contract. `StockStatusBadge` renders the raw numeric code, no guessed label/color      |

## Key flows

- **Warehouses**: `WarehousesListPage` (real `POST /warehouses/search`, keyword + pagination, plus the "open by id" quick-jump) → `WarehouseCreateDialog` → real `CreateWarehouse` → redirect to `WarehouseDetailPage` (Summary / Stock / Transactions / Metadata tabs, each future-extension-safe since they're independent `AppTabsContent` sections; the Stock/Transactions tabs show one page of up to 50 rows scoped to the warehouse, not the fully paginated view).
- **Stock**: `StockPage` — a product-scoped rollup lookup (pick a product/variation → real `GetProductStock`) plus a server-paginated table of inventory records (`POST /inventories/search`, filterable by warehouse) → `InventoryDetailPage`.
- **Inventory record detail**: `InventoryDetailPage` — `InventorySummaryCard` (cross-referencing Catalog for product/variation display names, Inventory for warehouse name — never re-displaying full Product data), Stock In/Out/Adjust actions (`StockActionDialog`, one component for all three since they share a `{quantity-ish, reason}` shape), a disabled Transfer placeholder, real `TransactionTimeline`, metadata.
- **Stock Transactions**: `StockTransactionsPage` — real `POST /inventory-transactions/search`, server-paginated and filterable by warehouse and by the three opaque `InventoryTransactionType` values.

## Reusable components (`shared/inventory/`)

`WarehouseSelector`, `InventorySummaryCard`, `StockStatusBadge`, `TransactionTimeline`, `InventoryToolbar`, `InventoryFilters`, `StockQuantity` — see [decisions/0016-shared-inventory-component-layer.md](../decisions/0016-shared-inventory-component-layer.md). All are prop-driven (no internal fetching), consistent with `shared/commerce`.

`StockQuantity`'s "In stock"/"Out of stock" badge is derived from the real `quantity` field (`> 0`), not an invented status.

## Audit

Reuses `shared/entity`'s `AuditTrailButton`/`AuditTrailDialog` with `service="Inventory"` — no Inventory-specific audit implementation, per the brief's explicit instruction.

## Dependencies on other modules

References **Product Management** (`useProductQuery` for product/variation display names on `InventoryDetailPage`) and **Product Categories/Tags** indirectly through Product — never duplicates Catalog data, only looks up a name for display next to a real inventory quantity.

## Open questions / backend dependencies

- If Warehouse ever gets Update/Delete endpoints, enable the currently-disabled buttons in `WarehousesListPage`.
- If `WarehouseStatus`/`InventoryTransactionType` ever get a published mapping, `shared/inventory/StockStatusBadge.tsx` is the only file to update.
- If a real Stock Transfer endpoint is added, `InventoryDetailPage`'s disabled Transfer button is the placeholder to replace.
