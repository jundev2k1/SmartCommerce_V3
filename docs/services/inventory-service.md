# Inventory Service

**Scope:** Inventory-specific facts and its documented divergences from the [User Service](user-service.md) reference implementation. General patterns live in [04-coding-rules.md](../04-coding-rules.md)/[02-architecture-rules.md](../02-architecture-rules.md) — not repeated here.

## Projects

`Inventory.Domain`, `Inventory.Application`, `Inventory.Infrastructure`, `Inventory.Persistence`, `Inventory.API` — same 5-layer split as User.

## Entities

- **Warehouse** (`Inventory.Domain/Entities/Warehouse.cs`) — unchanged: `Code` (unique), `Name`, `Address`, `Status`. Seeded with one `MAIN` / "Main Warehouse" row by `InventorySeeder`.
- **Inventory** (`Inventory.Domain/Entities/Inventory.cs`) — **stock-keeping unit is now `(ProductVariationId, WarehouseId)`, not `(ProductId, WarehouseId)`** — a Product can no longer exist without a variation, and a variation is the actual priced/stocked SKU. `ProductId` is still carried on the row (indexed, non-unique) purely so "total stock for a product" can filter without a cross-service join back to Product — it is never a second source of truth for `Quantity`. `Increase`/`Decrease`/`Adjust` unchanged. `IAuditable`, Aggregate Root for `InventoryTransaction`. Mapped with a Postgres `xmin` concurrency token (see "Concurrency" below) — guards `StockIn`/`StockOut`/`Adjust`/`DeductStock`/`RestockStock` all racing the same row.
- **InventoryTransaction** (`Inventory.Domain/Entities/InventoryTransaction.cs`) — gained `ProductVariationId` alongside the existing `ProductId`, otherwise unchanged (append-only movement log, one row per stock mutation in the same transaction as the `Inventory` update).
- **StockDeduction** (`Inventory.Domain/Entities/StockDeduction.cs`) — new. Idempotency ledger for the `DeductStock`/`RestockStock` gRPC pair, `Id` is the caller-supplied `deduction_id` (Order Service's `OrderId`), not a surrogate key. `Status` (`Succeeded`/`Failed`/`Reversed`), `ItemsJson` (snapshot of what was actually deducted, so `RestockStock` can reverse it without the caller resending item details), `FailureCode`, `Reason`. Not `IAuditable` — it's technical plumbing, not a business record. See "gRPC" below and [reference/create-order-saga.md](../reference/create-order-saga.md).

## Ports & routing

Internal `8080` (REST) **and gRPC (`5002` internal, `ASPNETCORE_GRPC_PORT`)** — Inventory now has a dual HTTP/gRPC listener, following User's exact pattern (`Program.cs` `ConfigureKestrel` binds both `Http1` on the REST port and `Http2` on the gRPC port). Gateway path prefix `/api/inventory/` (`RequireAuth: true`) for REST; gRPC is called directly (no gateway hop), same as User's gRPC surface.

## Routes (Carter endpoints, `Inventory.API/Endpoints/`)

| Method | Route | File | Purpose |
|---|---|---|---|
| POST | `/warehouses` | `CreateWarehouse.cs` | Create a warehouse (RequireAdmin) |
| GET | `/warehouses/{warehouseId}` | `GetWarehouse.cs` | Fetch a warehouse by id (RequireAdmin) |
| GET | `/inventories/{inventoryId}` | `GetInventory.cs` | Fetch current stock quantity, incl. `ProductVariationId` (RequireAuthenticated) |
| GET | `/inventories/{inventoryId}/history` | `GetInventoryHistory.cs` | List stock movement transactions for an inventory record (RequireAdmin) |
| POST | `/inventories/{inventoryId}/stock-in` | `StockIn.cs` | Increase stock, logs a StockIn transaction (RequireAdmin) |
| POST | `/inventories/{inventoryId}/stock-out` | `StockOut.cs` | Decrease stock, logs a StockOut transaction (RequireAdmin) |
| POST | `/inventories/{inventoryId}/adjust` | `AdjustStock.cs` | Directly correct stock to a known value, logs an Adjustment transaction (RequireAdmin) |
| GET | `/products/{productId}/stock` | `GetProductStock.cs` | `productId` alone → total stock across every variation/warehouse; `?productVariationId=` narrows to one variation. Same query the gRPC service is backed by. (RequireAuthenticated) |

## gRPC: `InventoryGrpcService`

`BuildingBlock.Contract/Protos/inventory.proto` defines three RPCs. `Inventory.API/GrpcServices/InventoryGrpcServiceImpl.cs` is a thin adapter for all three (parse request → dispatch a Query/Command via `ISender` → map result), following User's `UserGrpcServiceImpl` pattern exactly. Wired via `AddGrpcServer()` in `Inventory.API/DependencyInjection.cs` and `app.MapGrpcService<InventoryGrpcServiceImpl>()` in `ApplicationPipeline.cs`. Order Service is the only caller today (`Order.Infrastructure/GrpcClients/InventoryClientService.cs`) — see [Order Service](order-service.md#grpc-client-order--inventory) and [reference/create-order-saga.md](../reference/create-order-saga.md).

- **`GetProductStock(GetProductStockRequest) returns (GetProductStockResponse)`** — `product_variation_id` is `optional` (proto3 `optional`, generates `HasProductVariationId`); omitted/empty means the whole-product rollup. Read-only; dispatches the same `GetProductStockQuery` the REST endpoint uses. Called by `CreateOrderHandler`'s pre-check (never reserves).
- **`DeductStock(DeductStockRequest) returns (DeductStockResponse)`** — new. `deduction_id` is the idempotency key for the whole batch (Order passes its `OrderId`); all requested `(product_variation_id, quantity)` items deduct together or none do. Dispatches `DeductStockCommand` (`Inventory.Application/Features/Inventories/Commands/DeductStock/`): validates every item's current stock against the `MAIN` warehouse first, and only if *all* are sufficient does it call `Inventory.Decrease` per item + log an `InventoryTransaction` (`StockOut`) + persist a `StockDeduction` row, inside one `IUnitOfWork.ExecuteTransactionAsync`. A repeat call with the same `deduction_id` replays the stored `StockDeduction` outcome instead of decrementing twice. Retries up to 3 times internally on `ConflictException` (a concurrent writer touched the same `Inventory` row — see "Concurrency" below), re-validating quantities fresh each attempt rather than blindly retrying stale numbers.
- **`RestockStock(RestockStockRequest) returns (RestockStockResponse)`** — new. Compensating action, looked up by the same `deduction_id` — reverses a previously-`Succeeded` `StockDeduction` (`Inventory.Increase` per item from `ItemsJson` + an `InventoryTransaction` `StockIn` entry), marks the ledger row `Reversed`. Idempotent: reversing an already-`Reversed` (or never-`Succeeded`) `deduction_id` is a no-op `Success: true` — a compensating action must never itself become a blocking failure.

## Concurrency

`InventoryConfig` maps `Inventory`'s `xmin` (Postgres's native per-row system column) as an EF concurrency token via a shadow property + `IsRowVersion()` — guards `StockIn`/`StockOut`/`Adjust`/`DeductStock`/`RestockStock` racing the same row. `EfUnitOfWork.ExecuteTransactionAsync` (`BuildingBlock.Persistence.Ef`) translates the resulting `DbUpdateConcurrencyException` into an Application-layer `ConflictException`, so callers above Persistence never need to reference an EF-specific type. **Migration authors:** never let a scaffolded migration `AddColumn`/`DropColumn` an `xmin` column — Postgres rejects `ALTER TABLE ... ADD COLUMN xmin` outright ("column name xmin conflicts with a system column name"); hand-remove those lines, the model-level mapping alone is sufficient.

## Messaging: consumes three Product-originated integration events

`Inventory.Infrastructure/Messaging/Consumers/` — each deserializes → dispatches an internal event via `IInternalEventDispatcher`, no business logic in the consumer itself (per `docs/reference/events.md`'s DOs/DON'Ts):

- **`ProductVariationCreatedIntegrationEvent`** (topic `productvariationcreatedintegrationevent`) → `OnProductVariationCreatedEvent`/`Handler` — looks up the `MAIN` warehouse and creates a zero-stock `Inventory` row for the new variation. Checks `GetByVariationAndWarehouseAsync` first as an idempotency safety net beyond the Inbox dedup (a redelivered/replayed message must never create a second row for the same variation+warehouse). If `MAIN` is missing, logs a warning and skips (best-effort, same as before).
- **`ProductVariationDeletedIntegrationEvent`** → `OnProductVariationDeletedEvent`/`Handler` — deletes every Inventory row for that variation (across all warehouses); a deleted variation no longer exists to hold stock against.
- **`ProductDeletedIntegrationEvent`** → `OnProductDeletedEvent`/`Handler` — deletes every Inventory row for the whole product in one pass. Needed because whole-product deletion is an EF cascade over the owned `ProductVariation` rows on the Product side, so no per-variation Deleted event fires for each one individually.

This replaces the pre-redesign single `OnProductCreatedEvent` consuming `ProductCreatedIntegrationEvent.VariantId` — stock now keys off the variation-scoped events instead, since `ProductCreatedIntegrationEvent` no longer carries Sku/Price/variant info at all (see [Product Service](product-service.md#messaging-product--inventory--order-six-integration-events)).

## Inventory-specific building blocks (not present in User)

- **Dual Outbox+Inbox, like Order/User/Auth** — unchanged from before.
- **`IInventoryRepository`/`IInventoryTransactionRepository`/`IStockDeductionRepository` are not generic `IRepository<T>`** — `InventoryRepo` gained rollup/bulk methods beyond generic CRUD (`GetTotalStockByProductIdAsync`, `GetTotalStockByVariationIdAsync`, `DeleteByProductIdAsync`, `DeleteByVariationIdAsync`) and no longer implements `IRepository<T>`; `StockDeductionRepo` only ever needs id-keyed lookup/insert/update. All three are registered manually in `Inventory.Persistence/DependencyInjection.cs` rather than picked up by the Scrutor scan (`WarehouseRepo` is untouched and still auto-discovered).
- **No Redis / `ICacheService` usage** — unchanged.

## Naming note: the `Inventory` entity vs. the `Inventory` root namespace

Unchanged — `Inventory.Application`/`Inventory.Persistence` alias it:

```csharp
global using InventoryEntity = Inventory.Domain.Entities.Inventory;
```

## Known issues

- None yet.
