# 0016 — `shared/inventory/`: Reusable Inventory-Domain Components

**Status:** Accepted

**Date:** 2026-07-20 (Phase 5 — Inventory)

## Context

Inventory (Warehouses, Stock, Stock Transactions) needs a handful of domain-specific presentational pieces — a warehouse picker, a stock-record summary, an opaque status/type badge, a real transaction timeline, and filter/toolbar content — none of which fit `shared/entity` (admin-CRUD scaffolding), `shared/commerce` (customer-facing Shop/Cart/Checkout/Orders), or `shared/ui` (third-party wrappers). Inventory is also, per the brief, deliberately independent from Catalog — these components must not re-display full Product/Variant data, only reference it.

## Decision

Add `shared/inventory/`: `WarehouseSelector`, `InventorySummaryCard`, `StockStatusBadge`, `TransactionTimeline`, `InventoryToolbar`, `InventoryFilters`, `StockQuantity`. One barrel, same pattern as the three existing shared layers.

All components are prop-driven (no internal data fetching) — `features/inventory` owns every query (`useLocalWarehousesQuery`, `useLocalInventoryQuery`, etc.) and passes data in, keeping this layer purely presentational like `shared/commerce` rather than following `shared/entity/AuditTrailDialog`'s exception (justified there only because Audit has no feature module of its own yet).

## Rationale

- **`StockStatusBadge` is intentionally generic**, not two separate `WarehouseStatusBadge`/`TransactionTypeBadge` components: both `WarehouseStatus` (1–2) and `InventoryTransactionType` (1–3) are opaque enums with no published name mapping (see `docs/backend/inventory/README.md`), so both get the exact same treatment — render the raw code, one neutral badge variant, never a guessed color or label. This directly satisfies the brief's own "avoid hardcoded colors" instruction, which anticipated this ambiguity.
- **`InventoryToolbar` bakes in an "open by id" input**, not just `EntityToolbar`'s search+filters+actions shape — because no list endpoint exists anywhere in this contract (warehouses or inventory records), manually opening a known id is the _primary_ navigation method for this module, not an edge case. Extracting it once avoids re-implementing the same input+button pair on every Inventory page.
- **`InventorySummaryCard` shows only a name/SKU/warehouse-name/quantity**, never description/images/full category-tag lists — enforces the brief's "do not duplicate Product information already available in Catalog" at the component level, not just by convention.
- **`TransactionTimeline` is a real implementation**, unlike `shared/commerce`'s `OrderTimeline` (a literal placeholder) — `GetInventoryHistory` actually returns transaction data, so there's no reason to fake a placeholder here.

## Alternatives considered

- **Fold these into `shared/entity`**: rejected — `shared/entity` has no notion of "reference another bounded context's data without duplicating it," which is core to how these components behave.
- **Separate `WarehouseStatusBadge` and `TransactionTypeBadge`**: rejected — both are "opaque numeric code from this same service, no color, no guessed label," so splitting them would just be two copies of the same 8 lines.
- **Let `WarehouseSelector`/filters fetch their own data**: rejected — would require `shared/inventory` to import `features/inventory`, violating the one-directional `app → features → shared` dependency rule.

## Consequences

- If `WarehouseStatus`/`InventoryTransactionType` ever get a published mapping, `StockStatusBadge` is the only file to update.
- Any future Inventory-adjacent module (e.g. a real Stock Transfer feature, once/if the backend adds it) reaches for this layer first rather than re-deriving a warehouse picker or opaque-status badge.

## Related documents

[architecture/overview.md](../architecture/overview.md), [decisions/0014-shared-entity-component-layer.md](./0014-shared-entity-component-layer.md), [decisions/0015-shared-commerce-component-layer.md](./0015-shared-commerce-component-layer.md), [modules/inventory-management.md](../modules/inventory-management.md), [backend/inventory/README.md](../backend/inventory/README.md)
