# Progress — 2026-07-22

Status legend: `[ ]` not started · `[~]` in progress · `[b]` blocked · `[x]` done

- [x] **Task 1 — Verify AddVariation contract vs. Frontend Task 2.** Resolved: global SKU uniqueness confirmed intentional (user decision); real bug was a TOCTOU race in the pre-check-vs-insert, now fixed (`EfUnitOfWork` translates Postgres unique-violation to 409) and tested; 409 message now names the conflicting product.
- [x] **Task 2 — Notification list null category/type/title.** Fixed: `BsonImmutableValueObjectRegistrar` registers explicit BsonClassMaps (reflection-bound private constructors) for all 8 `Notification.Domain.ValueObjects` types, from Persistence only (Domain stays MongoDB-free). 9 regression tests. Historical pre-fix documents left as-is (dev data, by user decision).
- [x] **Task 3 — Order-owner/checkout flow backend readiness.** Address field implemented (`ShippingAddress` on `Order`/`CreateOrderCommand`/`AdminCreateOrderCommand`/`GetOrderResponse` + migration, not yet applied to a live DB). `GetOrder` ownership check added (403 unless Admin/Root or own order) — was flagged as a gap, user decided to fix now. Payment method + auto-complete-on-transfer deferred as its own follow-up task, by decision.

## Cross-repo dependencies

- Frontend Task 2 (add-variation) — unblocked, Task 1 resolved. No frontend fix needed; the 409 was correct behavior, now with a clearer message.
- Frontend Task 5 (order detail fields) needs no backend change — `GetOrderResponse` already has the fields (now including `shippingAddress` too), frontend just needs to catch up.
- Frontend Task 6 (checkout redesign) — Phase B's address blocker is now resolved; payment/auto-complete remains blocked pending a fresh, separately-scoped backend task.
