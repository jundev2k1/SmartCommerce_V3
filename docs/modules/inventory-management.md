# Inventory Management (Warehouses, Stock, Stock Transactions)

**Purpose:** Warehouse administration, stock lookup, and stock movement (in/out/adjust) — independent from Catalog, referencing products/variations by id only.

**Scope:** `features/inventory` and the `(admin)/warehouses`, `(admin)/inventory`, `(admin)/stock-transactions` routes, plus the `shared/inventory` component layer.

**Related documents:** [modules/overview.md](./overview.md), [backend/inventory/README.md](../backend/inventory/README.md), [decisions/0016-shared-inventory-component-layer.md](../decisions/0016-shared-inventory-component-layer.md), [state/zustand-strategy.md](../state/zustand-strategy.md).

**When to read:** Touching Warehouses/Stock/Stock Transactions, or wondering why there's no real "list warehouses" table.

**When to ignore:** Catalog work (Products/Categories/Tags) — Inventory only references product/variation ids, it never duplicates their data.

---

## Feature folder(s)

`src/features/inventory/`. Shared presentation in `src/shared/inventory/`. Client-only tracking state in `src/shared/stores/local-warehouses.store.ts` and `src/shared/stores/local-inventory-ids.store.ts`.

## Routes

`/warehouses`, `/warehouses/[warehouseId]`, `/inventory` (Stock), `/inventory/[inventoryId]`, `/stock-transactions`.

## Why this module looks the way it does: the contract is far more limited than any prior phase's

Confirmed with the project owner before implementation (this materially shapes almost every screen):

- **Warehouse**: only `CreateWarehouse` and `GetWarehouse` exist. **No Update, no Delete, no List, anywhere.**
- **Inventory**: only `GetInventory(id)` and `GetInventoryHistory(id)` exist — both require an id you must already have. **No endpoint ever returns or discovers an `inventoryId`** starting from a product or warehouse; `GetProductStock(productId, {productVariationId?})` only returns a rollup `totalQuantity`, never a per-warehouse breakdown or an inventory id.
- `WarehouseStatus` (1–2) and `InventoryTransactionType` (1–3) both have unpublished, unconfirmed value mappings.

Given that, every screen in this module was built around **"local-tracking + manual ID lookup"**: `shared/stores/local-warehouses.store.ts` remembers warehouse ids created from this browser (from `CreateWarehouse`'s response); `shared/stores/local-inventory-ids.store.ts` remembers inventory ids the admin has opened by hand or acted on via stock-in/out/adjust. Every list-like screen in this module is a **browser-scoped convenience view**, not an authoritative one — a warehouse or inventory record that exists in the backend but was never opened/created from this browser will not appear until someone pastes its id into the "Open by ID" box that `shared/inventory`'s `InventoryToolbar` provides on every list page.

## What's real vs. placeholder

| Requested (brief)                                                              | Status                        | Why                                                                                                                                                                                                           |
| ------------------------------------------------------------------------------ | ----------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Warehouse Create                                                               | ✅ Real                       | `CreateWarehouse` exists                                                                                                                                                                                      |
| Warehouse Detail                                                               | ✅ Real                       | `GetWarehouse` exists                                                                                                                                                                                         |
| Warehouse List/Search                                                          | ⚠️ Browser-scoped workaround  | No list endpoint — see above                                                                                                                                                                                  |
| Warehouse Update                                                               | ❌ Disabled + tooltip         | No endpoint at all                                                                                                                                                                                            |
| Warehouse Delete                                                               | ❌ Disabled + tooltip         | No endpoint at all                                                                                                                                                                                            |
| Stock: Current Stock                                                           | ✅ Real (rollup)              | `GetProductStock`/`GetInventory`                                                                                                                                                                              |
| Stock: Reserved / Available / Safety Stock                                     | ❌ Omitted, not faked         | No such fields anywhere in the contract                                                                                                                                                                       |
| Stock: Warehouse / Product / Variant reference                                 | ✅ Real                       | `GetInventoryResponse` has all three ids                                                                                                                                                                      |
| Stock: Status                                                                  | ❌ Omitted                    | `GetInventoryResponse` has no status field (only Warehouse does)                                                                                                                                              |
| Stock In / Out / Adjust                                                        | ✅ Real                       | `StockIn`/`StockOut`/`AdjustStock`                                                                                                                                                                            |
| Stock Transfer                                                                 | ❌ Disabled placeholder       | No transfer endpoint anywhere                                                                                                                                                                                 |
| Transaction History (per record)                                               | ✅ Real                       | `GetInventoryHistory`                                                                                                                                                                                         |
| Transaction search/pagination (across records)                                 | ⚠️ Browser-scoped aggregation | No "list all transactions" endpoint — aggregated client-side across locally-known ids, see `useTransactionsForInventoryIds`                                                                                   |
| Transaction "Operator"                                                         | ❌ Omitted                    | Not in `InventoryTransactionDto`                                                                                                                                                                              |
| Transaction "Reference"                                                        | Rendered as **Reason**        | The contract's actual field is a free-text `reason`, not a distinct "reference" concept — labeled honestly rather than invented                                                                               |
| Transaction "Quantity Before"                                                  | ❌ Omitted                    | `quantity`/`quantityAfter` have no confirmed sign/delta convention across StockIn/StockOut/Adjust (Adjust's request is an absolute `newQuantity`, not a delta) — computing a "before" value would be guessing |
| Transaction type badges (Stock In/Out/Adjustment/Reservation/Release/Transfer) | ⚠️ Opaque only                | The enum only has 3 unconfirmed values total — "Reservation"/"Release"/"Transfer" don't correspond to anything in this contract. `StockStatusBadge` renders the raw numeric code, no guessed label/color      |

## Key flows

- **Warehouses**: `WarehousesListPage` (locally-known + "open by id") → `WarehouseCreateDialog` → real `CreateWarehouse` → redirect to `WarehouseDetailPage` (Summary / Stock / Transactions / Metadata tabs, each future-extension-safe since they're independent `AppTabsContent` sections).
- **Stock**: `StockPage` — a product-scoped rollup lookup (pick a product/variation → real `GetProductStock`) plus a table of locally-known inventory records with search/filter (by warehouse) → `InventoryDetailPage`.
- **Inventory record detail**: `InventoryDetailPage` — `InventorySummaryCard` (cross-referencing Catalog for product/variation display names, Inventory for warehouse name — never re-displaying full Product data), Stock In/Out/Adjust actions (`StockActionDialog`, one component for all three since they share a `{quantity-ish, reason}` shape), a disabled Transfer placeholder, real `TransactionTimeline`, metadata. Visiting a record by id remembers it in `local-inventory-ids.store.ts` for future visits.
- **Stock Transactions**: `StockTransactionsPage` aggregates `GetInventoryHistory` across every locally-known inventory id (optionally scoped to one warehouse via the same filter), merges + sorts newest-first, supports client-side pagination and a type filter built from whatever type values are actually present in the fetched data.

## Reusable components (`shared/inventory/`)

`WarehouseSelector`, `InventorySummaryCard`, `StockStatusBadge`, `TransactionTimeline`, `InventoryToolbar`, `InventoryFilters`, `StockQuantity` — see [decisions/0016-shared-inventory-component-layer.md](../decisions/0016-shared-inventory-component-layer.md). All are prop-driven (no internal fetching), consistent with `shared/commerce`.

`StockQuantity`'s "In stock"/"Out of stock" badge is derived from the real `quantity` field (`> 0`), not an invented status.

## Audit

Reuses `shared/entity`'s `AuditTrailButton`/`AuditTrailDialog` with `service="Inventory"` — no Inventory-specific audit implementation, per the brief's explicit instruction.

## Dependencies on other modules

References **Product Management** (`useProductQuery` for product/variation display names on `InventoryDetailPage`) and **Product Categories/Tags** indirectly through Product — never duplicates Catalog data, only looks up a name for display next to a real inventory quantity.

## Open questions / backend dependencies

- If `Warehouse`/`Inventory` ever get list endpoints, `WarehousesListPage`/`StockPage`/`StockTransactionsPage` should be rebuilt against them directly — none of the surrounding architecture (dialogs, forms, mutations, detail pages) needs to change.
- If Warehouse ever gets Update/Delete endpoints, enable the currently-disabled buttons in `WarehousesListPage`.
- If `WarehouseStatus`/`InventoryTransactionType` ever get a published mapping, `shared/inventory/StockStatusBadge.tsx` is the only file to update.
- If a real Stock Transfer endpoint is added, `InventoryDetailPage`'s disabled Transfer button is the placeholder to replace.
