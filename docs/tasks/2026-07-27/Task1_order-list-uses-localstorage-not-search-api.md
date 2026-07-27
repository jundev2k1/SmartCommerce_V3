# Task 1: Order admin list/approve pages are built from localStorage, not the real backend search

**Status:** Fixed 2026-07-27.

## Resolution

Added `src/services/order/search-orders.ts` (`POST /orders/search`) and `useOrdersSearchQuery` (`src/features/orders/api/orders.queries.ts`). `OrdersListPage.tsx` and `OrderApproveListPage.tsx` — both Admin-only routes (`/orders`, `/orders/approve`, see `shared/config/nav.ts`) — now search the real backend with server-side pagination, a customer-name keyword search box, and Status/Customer-Phone/Created-Date filters.

`OrderHistoryListPage.tsx` ("My Orders", route `/orders/history`, no role restriction) was **deliberately left** on `useLocalOrdersQuery`/`GetOrder`, not switched to `useOrdersSearchQuery` as originally suggested below: `SearchOrders` is `RequireAdmin`-gated on the backend, and no customer-scoped "list my own orders" endpoint exists at all (checked every `Order.API/Endpoints/*.cs` file). Switching that page would 403 for every non-admin customer. This is a correction to this task's own suggested acceptance criteria, not an oversight.

Two other gaps found and fixed during implementation (not in the original acceptance criteria):

- `useOrderRealtimeUpdates` and the approve/cancel mutations only invalidated the per-order detail query. That was correct when "the list" was just a set of detail queries, but is no longer sufficient now that a real list query (`orderKeys.searches()`) exists — added invalidation there too, so status changes (live event or Approve/Cancel action) refresh the admin table without a manual refresh.
- The `status` filter value must be the backend enum's string name (e.g. `"Confirmed"`), not the numeric value used everywhere else in this app — `SearchOrders` parses it via C# `Enum.Parse`, not a numeric cast.

Sorting was narrowed to `createdAt` only (backend's `SearchOrders` only allows sorting by `customerName`/`status`/`createdAt`, not `totalAmount` — the old "Total" sort options were removed since they'd 400). The old "Customer ID" filter was replaced with "Customer Phone" per the acceptance criteria below (the backend doesn't support filtering by `customerId` at all, only `customerName`/`phone`/`status`/`createdAt`).

Task 12's stale-comment cleanup for `OrdersListPage.tsx:23` was done as part of this rewrite (the whole comment/hook it was attached to no longer exists). `OrderItems.tsx`'s stale comment is unrelated to this task (it's about the Discount/Variant columns, Task 6/12) and was left for that task.

## Source

Full-system business-requirements audit, 2026-07-27 (`docs/2026-07-27-audit-tasks.md`, now superseded by this per-task breakdown).

## Current state

`OrdersListPage.tsx:28-34`, `OrderHistoryListPage.tsx:22`, and `OrderApproveListPage.tsx:44` all source their data via `useLocalOrdersQuery` — a list of order IDs cached in the browser's `localStorage`, resolved one-by-one via `GetOrder`, then filtered/sorted client-side in JS (`OrdersListPage.tsx:49-85`). None of them call the backend's `POST /orders/search`, which already exists (`SearchOrders.cs`, `SearchOrdersHandler.cs`) and fully supports Created Date / Customer Name / Customer Phone filters server-side.

There is no `search-orders.ts` under `src/services/order/` at all.

Code comments in `OrdersListPage.tsx:23` and `OrderItems.tsx:23` are stale — they assert "no list endpoint exists," which was true when originally written but is no longer true (backend Task from 2026-07-22 confirmed a `SearchOrders` endpoint was later added; see SimpleShop `docs/tasks/2026-07-27/Task4_order-missing-total-quantity.md` and sibling tasks for the current backend state).

## Why this matters

This is a critical functional gap for admin order management: the feature _looks_ like it works — a table renders with data — but an admin using a different browser or session sees a completely different (empty or partial) order list, because the "list" is really just "orders this specific browser happens to remember creating/viewing." It is not a true system-wide admin view.

## Suggested acceptance criteria

- New `src/services/order/search-orders.ts` client wrapper calling `POST /orders/search`.
- `OrdersListPage.tsx`, `OrderHistoryListPage.tsx`, `OrderApproveListPage.tsx` switched to a `useOrdersSearchQuery` hook backed by the real endpoint, with server-side pagination.
- Filtering by Created Date, Customer Name, and Customer Phone works against the backend.
- An order created in a different browser/session is visible in the admin's list.
- Stale "no list endpoint" comments removed (see Task 12 in this folder).

**Cross-ref:** SimpleShop `docs/tasks/2026-07-27/` (Order-related backend tasks).
