# Tasks — Overall Progress

One line per still-open task, most recent date first. Per-task detail lives in each date folder — see [README.md](./README.md) for the convention.

## 2026-07-27

Source: full-system business-requirements audit (Product, Customer, Order+concurrency, Audit Log, Inventory) — read-only, no fixes applied yet. 12 tasks opened, all still open. See [2026-07-27/PROGRESS.md](./2026-07-27/PROGRESS.md) for detail.

- [ ] Task 1 — Customer (User) Delete endpoint does not exist (`2026-07-27/Task1_customer-delete-endpoint-missing.md`).
- [ ] Task 2 — Order concurrency token never round-trips through the API contract; real cross-session conflicts unprotected (`2026-07-27/Task2_order-concurrency-token-not-in-contract.md`).
- [ ] Task 3 — Inventory REST stock-mutation endpoints have no idempotency protection (`2026-07-27/Task3_inventory-rest-endpoints-missing-idempotency.md`).
- [ ] Task 4 — Order aggregate/DTOs missing `TotalQuantity` (`2026-07-27/Task4_order-missing-total-quantity.md`).
- [ ] Task 5 — OrderItem has no distinct Variant field (`2026-07-27/Task5_order-item-missing-variant-field.md`).
- [ ] Task 6 — Audit log query API has no server-side entity/actor filter (`2026-07-27/Task6_audit-log-missing-entity-filter.md`).
- [ ] Task 7 — Inventory publishes no domain-specific stock events over Kafka (`2026-07-27/Task7_inventory-no-domain-stock-events-on-kafka.md`).
- [ ] Task 8 — No standardized retry policy (Polly) anywhere (`2026-07-27/Task8_no-standardized-retry-policy.md`).
- [ ] Task 9 — Dead-letter handling is a DB status flag only, not a real Kafka DLQ (`2026-07-27/Task9_dead-letter-handling-db-flag-only.md`).
- [ ] Task 10 — Category/Tag delete has no usage-count precheck (`2026-07-27/Task10_category-tag-delete-no-usage-precheck.md`).
- [ ] Task 11 — `RebuildProductSearchIndex` has no documented auth requirement (`2026-07-27/Task11_rebuild-search-index-auth-undocumented.md`).
- [ ] Task 12 — Inventory reservation model decision needed: deduct-immediately + saga-compensate vs. true reserve/commit/release (`2026-07-27/Task12_inventory-reservation-model-decision.md`).

## 2026-07-22

- [x] Task 1 — Verify AddVariation contract vs. Frontend Task 2 (`2026-07-22/Task1_verify-add-variation-contract.md`) — resolved: global SKU uniqueness intentional, TOCTOU race fixed, error message improved.
- [x] Task 2 — Notification list null category/type/title (`2026-07-22/Task2_notification-list-null-fields.md`) — fixed: BsonClassMap registration for all 8 affected value objects, tested.
- [x] Task 3 — Order-owner/checkout flow backend readiness (`2026-07-22/Task3_order-owner-checkout-flow-review.md`) — address field + GetOrder ownership check implemented; payment method deferred as its own follow-up task.

All three closed for this date. See [2026-07-22/PROGRESS.md](./2026-07-22/PROGRESS.md) for detail. Remaining open item: payment method + auto-complete-on-transfer needs a fresh backend task when scoped (see Task 3).
