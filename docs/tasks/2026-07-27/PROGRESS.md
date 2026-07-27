# Progress — 2026-07-27

Status legend: `[ ]` not started · `[~]` in progress · `[b]` blocked · `[x]` done

Source: full-system business-requirements audit covering Product, Customer, Order (incl. concurrency), Audit Log, and Inventory — read-only, no fixes applied. See root-level per-feature summary this audit produced (shared with the user directly) for the completion-percentage breakdown; this folder only tracks the resulting remaining-work items.

- [ ] **Task 1 — Customer (User) Delete endpoint does not exist.** Orphaned `DeleteUserCommand`/`Handler`, no `User.API` route, frontend delete button already disabled in anticipation.
- [ ] **Task 2 — Order concurrency token never round-trips through the API contract.** `xmin` + 409 exist at the DB/UoW level, but no command/DTO carries a client-echoed version — the actual "two users editing the same order" scenario is unprotected.
- [ ] **Task 3 — Inventory REST stock-mutation endpoints (AdjustStock/StockIn/StockOut) have no idempotency protection.** Existing Redis-backed framework isn't wired here; `inventory-api` has no Redis dependency yet either.
- [ ] **Task 4 — Order aggregate/DTOs missing `TotalQuantity` entirely.**
- [ ] **Task 5 — OrderItem has no distinct Variant field.**
- [ ] **Task 6 — Audit log query API has no server-side filter by entity/actor**, forcing a fragile client-side pagination workaround.
- [ ] **Task 7 — Inventory publishes no domain-specific stock events over Kafka**; Order↔Inventory integration is gRPC-only.
- [ ] **Task 8 — No standardized retry policy (Polly)** anywhere in the solution.
- [x] **Task 9 — Dead-letter handling is a DB status flag only**, not a real Kafka DLQ with reprocessing/alerting. Resolved: added `IInboxStore.GetDeadLetterSummaryAsync` (Ef+Mongo, all 7 services) plus `InboxDeadLetterMonitorJob`, a recurring job that logs a warning per stuck (consumer, topic) group every 15 min. Full replay-from-topic DLQ intentionally left out of scope.
- [ ] **Task 10 — Category/Tag delete has no usage-count precheck** before a 409.
- [ ] **Task 11 — `RebuildProductSearchIndex` has no documented auth requirement.**
- [ ] **Task 12 — Inventory's "reservation" is deduct-immediately + saga-compensate, not a true reserve/commit/release model** — decision needed on whether that's sufficient long-term.

## Cross-repo dependencies

- Frontend Task 4 (order concurrency conflict UI) is blocked on this folder's Task 2.
- Frontend Task 5 (customer delete button) is blocked on this folder's Task 1.
- Frontend Task 8 (audit trail client-side pagination) is blocked on this folder's Task 6.
- Frontend Tasks 9/10 (TotalQuantity/Variant display) are blocked on this folder's Tasks 4/5 respectively.
- Frontend Task 1 (order list via localStorage) and Task 2 (customer phone search UI) need **no backend work** — both endpoints already exist and work; frontend just needs to call them.
