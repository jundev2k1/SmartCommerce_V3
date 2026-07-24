# Progress — 2026-07-22

Status legend: `[ ]` not started · `[~]` in progress · `[b]` blocked · `[x]` done

- [x] **Task 1 — Audit trail dialog empty despite success.** Fixed: bounded multi-page search (up to 5 pages) plus a distinct "search truncated" empty state. Server-side `entityId` filter remains the real long-term fix (backlog.md §5).
- [x] **Task 2 — Cannot create variation (`AddVariation`).** Resolved on the backend — global SKU uniqueness confirmed intentional, a real TOCTOU race found and fixed, error message improved. No frontend fix needed.
- [x] **Task 3 — Create-product UI: only one variation at creation.** Implemented multi-variation UI with add/remove rows, default-variation radio button, full per-variation fields via useFieldArray + Controller.
- [x] **Task 4 — Category/Tag missing audit trail.** Built detail drawers (Option B) for both entities with AuditTrailButton wired in. Used `service="Product"` (not "Category"/"Tag") since service is the backend service name, matching every other AuditTrailButton call site — pending backend confirmation.
- [x] **Task 5 — Order Detail missing customer name/phone etc.** Fixed: updated GetOrderResponse type + OrderCustomer.tsx to render name/phone + backend docs. shippingAddress/cancellationReason/discount not surfaced yet (separate UI decisions needed).
- [x] **Task 6 — Checkout flow redesign (multi-step, editable order owner).** Phase A done: `CheckoutPage` is now a 4-step wizard (Cart → Owner info → Confirm → Complete) with a real, editable owner-info form (name/phone/address, prefilled from the account but no longer forced equal to it). `CreateOrderRequest`/`AdminCreateOrderRequest` gained the required `shippingAddress` field to match backend. `tsc`/`eslint` clean; no live backend in this sandbox to click-through end-to-end, so verify against a real backend before shipping.
  - [b] Payment blocked — see backend Task 3 (deferred as its own follow-up, not yet scoped).
- [x] **Task 7 — Inventory/Warehouse/Stock/Notification pages "not integrated."** All 3 causes fixed: built the real `/notifications` history page (`NotificationsHistoryPage.tsx`); deleted the broken `.env.local` (empty-string overrides were disabling SignalR + dev mocks); Warehouse/Inventory/Stock-Transaction lists switched over from browser-local id tracking to the real `/warehouses/search`, `/inventories/search`, `/inventory-transactions/search` endpoints (SimpleShop `docs/tasks/2026-07-22/Task5_warehouse-inventory-list-search-endpoints.md`) — `local-warehouses.store.ts`/`local-inventory-ids.store.ts` deleted.
- [x] **Task 8 — Users page: split into Admin/Normal User tabs.** Unblocked 2026-07-24 — backend added `Roles` to `/users/search` results plus a `role` filter (SimpleShop `docs/tasks/2026-07-22/Task4_search-users-endpoint-already-exists.md`, denormalize option). Implemented `searchUsers` client function + `useUsersSearchQuery` + `AppTabs`-based Admin/Normal-User split in `UsersPage.tsx`, each tab independently searchable/paginated server-side.
- [x] **Task 9 — Product search missing "Clear filters."** Fixed: Reset button infrastructure was already in FilterPanel; updated active check to include sortBy/sortDescending.

## Cross-repo dependencies

- Task 2 resolved via SimpleShop Task 1 (2026-07-22).
- Task 5 needs no backend work (already resolved server-side, now including `shippingAddress` too) — just catch up this repo's types/UI.
- Task 6's address blocker resolved via SimpleShop Task 3 (2026-07-22); payment remains blocked on a not-yet-scoped follow-up task.
- Task 8 fully unblocked and implemented as of 2026-07-24 — `/users/search` now returns `Roles` and supports a `role` filter (SimpleShop Task 4); frontend tabs shipped same day.
- Task 7 item 2 fully unblocked and implemented as of 2026-07-24 — SimpleShop Task 5 built all three search endpoints (`/warehouses/search`, `/inventories/search`, `/inventory-transactions/search`); frontend switch-over shipped same day.
