# Tasks — Overall Progress

One line per still-open task, most recent date first. Per-task detail lives in each date folder — see [README.md](./README.md) for the convention.

## 2026-07-22

- [x] Task 1 — Verify AddVariation contract vs. Frontend Task 2 (`2026-07-22/Task1_verify-add-variation-contract.md`) — resolved: global SKU uniqueness intentional, TOCTOU race fixed, error message improved.
- [x] Task 2 — Notification list null category/type/title (`2026-07-22/Task2_notification-list-null-fields.md`) — fixed: BsonClassMap registration for all 8 affected value objects, tested.
- [x] Task 3 — Order-owner/checkout flow backend readiness (`2026-07-22/Task3_order-owner-checkout-flow-review.md`) — address field + GetOrder ownership check implemented; payment method deferred as its own follow-up task.

All three closed for this date. See [2026-07-22/PROGRESS.md](./2026-07-22/PROGRESS.md) for detail. Remaining open item: payment method + auto-complete-on-transfer needs a fresh backend task when scoped (see Task 3).
