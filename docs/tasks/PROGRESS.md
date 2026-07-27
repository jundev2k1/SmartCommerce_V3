# Tasks — Overall Progress

One line per still-open task, most recent date first. Per-task detail lives in each date folder — see [README.md](./README.md) for the convention.

## 2026-07-27

Source: full-system business-requirements audit (Product, Customer, Order+concurrency, Audit Log, Inventory) — read-only, no fixes applied yet. 12 tasks opened; Task 9 resolved same day. A second audit pass this date (SmartCommerce V3 Search/Variation/Cart-Stock checklist) opened Tasks 13-22 — all 10 resolved same day. See [2026-07-27/PROGRESS.md](./2026-07-27/PROGRESS.md) for detail.

- [ ] Task 1 — Customer (User) Delete endpoint does not exist (`2026-07-27/Task1_customer-delete-endpoint-missing.md`).
- [ ] Task 2 — Order concurrency token never round-trips through the API contract; real cross-session conflicts unprotected (`2026-07-27/Task2_order-concurrency-token-not-in-contract.md`).
- [ ] Task 3 — Inventory REST stock-mutation endpoints have no idempotency protection (`2026-07-27/Task3_inventory-rest-endpoints-missing-idempotency.md`).
- [ ] Task 4 — Order aggregate/DTOs missing `TotalQuantity` (`2026-07-27/Task4_order-missing-total-quantity.md`).
- [ ] Task 5 — OrderItem has no distinct Variant field (`2026-07-27/Task5_order-item-missing-variant-field.md`).
- [ ] Task 6 — Audit log query API has no server-side entity/actor filter (`2026-07-27/Task6_audit-log-missing-entity-filter.md`).
- [ ] Task 7 — Inventory publishes no domain-specific stock events over Kafka (`2026-07-27/Task7_inventory-no-domain-stock-events-on-kafka.md`).
- [ ] Task 8 — No standardized retry policy (Polly) anywhere (`2026-07-27/Task8_no-standardized-retry-policy.md`).
- [x] Task 9 — Dead-letter handling is a DB status flag only, not a real Kafka DLQ (`2026-07-27/Task9_dead-letter-handling-db-flag-only.md`) — resolved: periodic count-and-log dead-letter monitor job added; full replay-from-topic DLQ left out of scope.
- [ ] Task 10 — Category/Tag delete has no usage-count precheck (`2026-07-27/Task10_category-tag-delete-no-usage-precheck.md`).
- [ ] Task 11 — `RebuildProductSearchIndex` has no documented auth requirement (`2026-07-27/Task11_rebuild-search-index-auth-undocumented.md`).
- [ ] Task 12 — Inventory reservation model decision needed: deduct-immediately + saga-compensate vs. true reserve/commit/release (`2026-07-27/Task12_inventory-reservation-model-decision.md`).
- [x] Task 13 — Keyword/filter search has no case-insensitive option, shared infra + User + Order (`2026-07-27/Task13_case-insensitive-search-missing.md`) — resolved.
- [x] Task 14 — Order search has no Order ID field (`2026-07-27/Task14_order-id-search-filter-missing.md`) — resolved.
- [x] Task 15 — Product Search doesn't index/search Variation Name (`2026-07-27/Task15_product-search-missing-variation-name.md`) — resolved; live reindex not run this session.
- [x] Task 16 — `ProductVariation.Name` is a dead property, never set or returned (`2026-07-27/Task16_variation-name-not-wired.md`) — resolved: full Create/Add/Update/Read wiring + migration.
- [x] Task 17 — No shared stock-availability service across Cart/Order (`2026-07-27/Task17_no-shared-stock-validation-service.md`) — resolved: `IStockAvailabilityService` extracted.
- [x] Task 18 — Add Cart API never checks real stock, only a status flag (`2026-07-27/Task18_add-cart-no-realtime-stock-check.md`) — resolved.
- [x] Task 19 — Get Cart returns no live stock/availability per item (`2026-07-27/Task19_get-cart-no-live-stock-check.md`) — resolved.
- [x] Task 20 — Create Order's insufficient-stock error has no structured per-item detail (`2026-07-27/Task20_create-order-insufficient-stock-error-not-structured.md`) — resolved as part of Task 17's change.
- [x] Task 21 — Product Search's `isInStock` reflects only the default variation (`2026-07-27/Task21_product-list-add-cart-checks-default-variation-only.md`) — resolved via direct gRPC (user-confirmed approach).
- [x] Task 22 — GetProduct response has no per-variation stock data (`2026-07-27/Task22_get-product-no-per-variation-stock.md`) — resolved.
- [ ] Task 23 — UpdateOrder fails unconditionally, not just under concurrency (`2026-07-27/Task23_updateorder-always-fails-not-a-race-condition.md`) — discovered while building `Order.IntegrationTests`; new OrderItems get misclassified Modified instead of Added, causing every UpdateOrder call to 409. Not fixed (out of scope for a test-writing task).

## 2026-07-22

- [x] Task 1 — Verify AddVariation contract vs. Frontend Task 2 (`2026-07-22/Task1_verify-add-variation-contract.md`) — resolved: global SKU uniqueness intentional, TOCTOU race fixed, error message improved.
- [x] Task 2 — Notification list null category/type/title (`2026-07-22/Task2_notification-list-null-fields.md`) — fixed: BsonClassMap registration for all 8 affected value objects, tested.
- [x] Task 3 — Order-owner/checkout flow backend readiness (`2026-07-22/Task3_order-owner-checkout-flow-review.md`) — address field + GetOrder ownership check implemented; payment method deferred as its own follow-up task.

All three closed for this date. See [2026-07-22/PROGRESS.md](./2026-07-22/PROGRESS.md) for detail. Remaining open item: payment method + auto-complete-on-transfer needs a fresh backend task when scoped (see Task 3).
