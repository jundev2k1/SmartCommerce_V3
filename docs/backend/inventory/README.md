# Backend: Inventory Service

**Purpose:** What the Inventory Service exposes and how `src/services/inventory` maps to it.

**Scope:** This service's contract only.

**Related documents:** [backend/README.md](../README.md), [backend/feature-mapping.md](../feature-mapping.md), [backend/coverage-report.md](../coverage-report.md), `src/services/inventory/`

**When to read:** Building Inventory/Warehouses/Stock Transactions (Phase 9).

**When to ignore:** Any work not touching stock/warehouses.

---

## Purpose

Stock quantity tracking per product-variation-per-warehouse, warehouse management, and the stock-movement transaction log (stock in / stock out / adjustment). Also backs the gRPC stock lookup the Order Service consumes internally (irrelevant to the frontend — noted here only because `GetProductStock`'s description mentions it).

## Base path

`/api/inventory`.

## Auth requirements

Not explicitly documented per-endpoint.

## Endpoints

| Operation                | Method & Path                               | Client function                      |
| ------------------------ | ------------------------------------------- | ------------------------------------ |
| Get inventory            | `GET /inventories/{inventoryId}`            | `getInventory(inventoryId)`          |
| Get inventory history    | `GET /inventories/{inventoryId}/history`    | `getInventoryHistory(inventoryId)`   |
| Get product stock rollup | `GET /products/{productId}/stock`           | `getProductStock(productId, params)` |
| Stock in                 | `POST /inventories/{inventoryId}/stock-in`  | `stockIn(inventoryId, request)`      |
| Stock out                | `POST /inventories/{inventoryId}/stock-out` | `stockOut(inventoryId, request)`     |
| Adjust stock             | `POST /inventories/{inventoryId}/adjust`    | `adjustStock(inventoryId, request)`  |
| Create warehouse         | `POST /warehouses`                          | `createWarehouse(request)`           |
| Get warehouse            | `GET /warehouses/{warehouseId}`             | `getWarehouse(warehouseId)`          |

## Request/response DTOs

Each endpoint file in `src/services/inventory/` declares its own request/response DTOs inline (see [decisions/0012-backend-service-client-layer.md](../../decisions/0012-backend-service-client-layer.md)); enum-like types live in `.../types/`. Notable: `GetProductStockResponse.totalQuantity` rolls up across every warehouse/variation unless `productVariationId` narrows it.

## Pagination style

N/A — no list endpoints in this contract (no "list warehouses," no "list inventories" — see limitations below).

## Error responses

Prose-only per endpoint: mostly 404 (not found) and 400 (validation/insufficient stock for `stock-out`). No formal error schema.

## Known limitations / TODOs

- [ ] **No list endpoints.** Neither warehouses nor inventory records have a `GET` list/search endpoint in this contract — only get-by-id. The Warehouses and Inventory frontend modules (Phase 9) will need list endpoints before their table views can be built; flagged in [coverage-report.md](../coverage-report.md).
- [ ] `WarehouseStatus` (1, 2) has no published name mapping — kept opaque.
- [ ] `InventoryTransactionType` (1, 2, 3) has no published name mapping. The service groups `StockIn`/`StockOut`/`Adjust` endpoints in that order in the Swagger document, loosely suggesting 1=StockIn/2=StockOut/3=Adjustment, but this is inferred from section ordering only, not confirmed — do not rely on it without backend confirmation.
