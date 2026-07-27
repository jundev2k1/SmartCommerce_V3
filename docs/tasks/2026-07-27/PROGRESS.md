# Progress — 2026-07-27

Status legend: `[ ]` not started · `[~]` in progress · `[b]` blocked · `[x]` done

Source: full-system business-requirements audit covering Product, Customer, Order (incl. concurrency), Audit Log, and Inventory — read-only, no fixes applied. See the SimpleShop backend repo's own `docs/tasks/2026-07-27/` for the backend-side counterpart tasks.

- [x] **Task 1 — Order admin list/approve pages are built from `localStorage`, not the real `SearchOrders` endpoint.** Fixed: new `search-orders.ts` client + `useOrdersSearchQuery`; `OrdersListPage`/`OrderApproveListPage` (both Admin-only routes) now search the real backend with server-side pagination, status/phone/created-date filters, and customer-name keyword search. `OrderHistoryListPage` intentionally left on `useLocalOrdersQuery` — `SearchOrders` is `RequireAdmin`-gated and no customer-scoped "list my orders" endpoint exists, so a non-admin hitting it would 403; that page's route has no role restriction. Also fixed a related gap found in review: `useOrderRealtimeUpdates`/approve-cancel mutations only invalidated the per-order detail query, which no longer feeds the new list query — added `orderKeys.searches()` invalidation so status changes refresh the table live.
- [ ] **Task 2 — Customer search by phone (prefix + suffix) has no UI at all.** No backend work needed — fully indexed prefix/suffix search already exists server-side.
- [ ] **Task 3 — Order edit UI only supports owner info** — no item-update or delete client exists, though both endpoints exist on the backend.
- [b] **Task 4 — No conflict-resolution UI for concurrent order edits.** Blocked on SimpleShop Task 2 (version token must exist in the contract first).
- [b] **Task 5 — Customer delete button permanently disabled.** Blocked on SimpleShop Task 1.
- [ ] **Task 6 — Order Detail line items omit the Discount column**, despite it already being fetched from the API.
- [ ] **Task 7 — Order list table shows raw `customerId` instead of `customerName`.**
- [b] **Task 8 — Audit trail dialog's client-side pagination workaround will silently miss data at scale.** Blocked on SimpleShop Task 6 for the real fix; the stray `console.log` cleanup is unblocked.
- [b] **Task 9 — Display `TotalQuantity` once it exists on the backend.** Blocked on SimpleShop Task 4.
- [b] **Task 10 — Render `Variant` once it exists on the backend OrderItem DTO.** Blocked on SimpleShop Task 5.
- [ ] **Task 11 — `RebuildProductSearchIndex` frontend client is dead code** (never called from any component).
- [~] **Task 12 — Stale code comments** claiming endpoints/fields don't exist when they now do (or partially do). `OrdersListPage.tsx` comment gone via Task 1; `OrderItems.tsx` comment still blocked on Task 6.

## Cross-repo dependencies

- Tasks 4, 5, 8, 9, 10 are blocked on the corresponding SimpleShop backend task of the same theme (see `SimpleShop/docs/tasks/2026-07-27/`).
- Tasks 2, 3, 6, 7, 11, 12 need no backend work — pure frontend catch-up or cleanup.
- Task 1 done as of 2026-07-27 — no backend work was needed, `SearchOrders` already existed.
