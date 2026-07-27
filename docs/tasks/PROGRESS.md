# Tasks — Overall Progress

One line per still-open task, most recent date first. Per-task detail lives in each date folder — see [README.md](./README.md) for the convention.

## 2026-07-27

Source: full-system business-requirements audit (Product, Customer, Order+concurrency, Audit Log, Inventory) — read-only, no fixes applied yet. 12 tasks opened; 1 fixed, 5 blocked on the paired SimpleShop backend task, rest still open. See [2026-07-27/PROGRESS.md](./2026-07-27/PROGRESS.md) for detail.

- [x] Task 1 — Order admin list/approve pages built from `localStorage`, not the real `SearchOrders` endpoint (`2026-07-27/Task1_order-list-uses-localstorage-not-search-api.md`) — fixed: `OrdersListPage`/`OrderApproveListPage` now use `searchOrders`/`useOrdersSearchQuery` with server-side pagination and filters; `OrderHistoryListPage` intentionally kept on local-orders (customer route, `SearchOrders` is Admin-only).
- [ ] Task 2 — Customer search by phone (prefix + suffix) has no UI at all (`2026-07-27/Task2_customer-phone-search-ui-missing.md`).
- [ ] Task 3 — Order edit UI missing item-update and delete (`2026-07-27/Task3_order-edit-missing-item-update-and-delete.md`).
- [b] Task 4 — No conflict-resolution UI for concurrent order edits; blocked on SimpleShop Task 2 (`2026-07-27/Task4_order-concurrency-conflict-ui-missing.md`).
- [b] Task 5 — Customer delete button permanently disabled; blocked on SimpleShop Task 1 (`2026-07-27/Task5_customer-delete-button-disabled.md`).
- [ ] Task 6 — Order Detail line items omit the Discount column (`2026-07-27/Task6_order-detail-discount-column-missing.md`).
- [ ] Task 7 — Order list table shows raw `customerId` instead of `customerName` (`2026-07-27/Task7_order-list-shows-customerid-not-name.md`).
- [b] Task 8 — Audit trail dialog's client-side pagination workaround will silently miss data at scale; blocked on SimpleShop Task 6 (`2026-07-27/Task8_audit-trail-dialog-client-side-pagination-risk.md`).
- [b] Task 9 — Display `TotalQuantity` once it exists on the backend; blocked on SimpleShop Task 4 (`2026-07-27/Task9_order-total-quantity-display.md`).
- [b] Task 10 — Render `Variant` once it exists on the backend; blocked on SimpleShop Task 5 (`2026-07-27/Task10_order-item-variant-display.md`).
- [ ] Task 11 — `RebuildProductSearchIndex` frontend client is dead code (`2026-07-27/Task11_rebuild-search-index-dead-code.md`).
- [~] Task 12 — Stale code comments claiming missing endpoints/fields (`2026-07-27/Task12_stale-code-comments-order-list-and-items.md`) — `OrdersListPage.tsx` part fixed via Task 1; `OrderItems.tsx` part still blocked on Task 6.

## 2026-07-22

- [x] Task 1 — Audit trail dialog empty despite success (`2026-07-22/Task1_audit-trail-dialog-empty.md`) — fixed, bounded multi-page search + distinct empty state.
- [x] Task 2 — Cannot create variation (`2026-07-22/Task2_add-variation-cannot-create.md`) — resolved on the backend, no frontend fix needed.
- [x] Task 3 — Create-product UI single-variation gap (`2026-07-22/Task3_create-product-multi-variation-ui.md`) — implemented multi-variation UI with useFieldArray + Controller.
- [x] Task 4 — Category/Tag missing audit trail (`2026-07-22/Task4_category-tag-audit-trail-missing.md`) — built detail drawers with AuditTrailButton, service="Product" per established convention.
- [x] Task 5 — Order Detail missing customer fields (`2026-07-22/Task5_order-detail-missing-fields.md`) — updated types and UI to render customerName/customerPhone.
- [b] Task 6 — Checkout flow redesign (`2026-07-22/Task6_checkout-flow-redesign.md`) — Phase A + address done; payment method remains blocked on a not-yet-scoped backend task.
- [x] Task 7 — Inventory/Warehouse/Stock/Notification integration gaps (`2026-07-22/Task7_inventory-warehouse-notification-integration-gaps.md`) — fixed: notifications page, env vars, and Warehouse/Inventory/Stock lists all switched to real backend search endpoints.
- [x] Task 8 — Users page Admin/Normal-User tabs (`2026-07-22/Task8_users-page-role-tabs.md`) — unblocked and implemented: two-tab UI backed by `/users/search`'s new `role` filter.
- [x] Task 9 — Product search "Clear filters" (`2026-07-22/Task9_product-search-clear-filters.md`) — fixed, updated active check to include sortBy/sortDescending.

See [2026-07-22/PROGRESS.md](./2026-07-22/PROGRESS.md) for issue/caution detail on each.
