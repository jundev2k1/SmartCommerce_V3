# Progress — 2026-07-27

Status legend: `[ ]` not started · `[~]` in progress · `[b]` blocked · `[x]` done

Source: full-system business-requirements audit covering Product, Customer, Order (incl. concurrency), Audit Log, and Inventory — read-only, no fixes applied. See root-level per-feature summary this audit produced (shared with the user directly) for the completion-percentage breakdown; this folder only tracks the resulting remaining-work items. A second pass this date (SmartCommerce V3 Search/Variation/Cart-Stock checklist, Tasks 13-22) was fully implemented.

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

### SmartCommerce V3 Search/Variation/Cart-Stock checklist (same date, second audit pass)

- [x] **Task 13 — Keyword/filter search has no case-insensitive option** (shared `BuildingBlock.Criteria` infra + User Email/UserName + Order CustomerName). Resolved: `IgnoreCase` field/fluent modifier added, wired into both services. Functional DB index left as a follow-up.
- [x] **Task 14 — Order search has no Order ID field at all.** Resolved: exact-match `id` field added to `OrderCriteriaDefinition`.
- [x] **Task 15 — Product Search does not index or search Variation Name.** Resolved: `VariationNames` added to the ES document/mapping/query. Live reindex not run this session (Docker wasn't up) — deploy must trigger `RebuildProductSearchIndex`.
- [x] **Task 16 — `ProductVariation.Name` is a dead property** — never set (no `Create()` param, no rename method) and never returned by any DTO. Resolved: fully wired Create/Add/Update/Read + migration; `VariationName` added additively to the two Product integration events (Order not yet consuming it). Enables Task 15.
- [x] **Task 17 — No shared stock-availability service** — Create Order's real Inventory check and Cart's status-only check are separate, unshared logic. Resolved (Order-side): `IStockAvailabilityService` extracted, `OrderItemPreparationService` now depends on it. Cart consumption is Tasks 18/19.
- [x] **Task 18 — Add Cart API never checks real stock**, only a Product-status flag. Resolved: `EnsureOrderableAndInStockAsync` checks resulting quantity via `IStockAvailabilityService`; same structured 400 shape as Task 20.
- [x] **Task 19 — Get Cart returns no live stock/availability state per item.** Resolved: `CartItemResponse` gained `AvailableStock`/`IsInsufficientStock`, populated live per line; insufficient lines kept, not pruned.
- [x] **Task 20 — Create Order's insufficient-stock error has no structured per-item detail** — resolved as part of Task 17's change: `InsufficientAmountException`/`ExceptionFactory.InsufficientStock` now carry a `detail` payload (`{ insufficients: [...] }`).
- [x] **Task 21 — Product Search's `isInStock` reflects only the default variation**, not "any variation in stock." Resolved via direct gRPC (user-confirmed approach): `VariationIds` added to the search document, `IsInStock` now ORs stock across every Active variation.
- [x] **Task 22 — GetProduct response has no per-variation stock data** at all. Resolved: `ProductVariationResponse.AvailableStock` populated per-variation via the same fail-open Inventory batch pattern as Search.

## Cross-repo dependencies

- Frontend Task 4 (order concurrency conflict UI) is blocked on this folder's Task 2.
- Frontend Task 5 (customer delete button) is blocked on this folder's Task 1.
- Frontend Task 8 (audit trail client-side pagination) is blocked on this folder's Task 6.
- Frontend Tasks 9/10 (TotalQuantity/Variant display) are blocked on this folder's Tasks 4/5 respectively.
- Frontend Task 1 (order list via localStorage) and Task 2 (customer phone search UI) need **no backend work** — both endpoints already exist and work; frontend just needs to call them.
- Frontend Tasks 13-17 (second audit pass) all resolved 2026-07-27, each paired with this folder's Tasks 14, 16, 21, 22, and 18/20 respectively — all landed the same session, nothing left blocked.
