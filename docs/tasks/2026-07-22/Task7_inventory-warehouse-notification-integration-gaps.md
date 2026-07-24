# Task 7: Inventory, Warehouses, Stock Transactions, Notification pages not integrated

**Status:** All three items fixed. Item 2's backend blocker was resolved 2026-07-24 (SimpleShop `docs/tasks/2026-07-22/Task5_warehouse-inventory-list-search-endpoints.md`) and the frontend switch-over to the real search endpoints is now done too — see "Item 2" below.

## Ask

These pages reportedly can't show, search, or create data via the APIs.

## Investigation

`src/shared/config/nav.ts` marks all of `inventory`, `warehouses`, `stock-transactions` (:117-142), and `notifications` (:148-155) as `implemented: true`.

**These pages are real, not stubs** — with one exception:

- `src/app/(admin)/notifications/page.tsx:1-12` — the full-history `/notifications` route **is** `PlaceholderModulePage` despite `nav.ts` claiming `implemented: true`. Only the topbar bell/dropdown (`NotificationBell.tsx`, `NotificationDropdown.tsx`, `NotificationDetailDialog.tsx`) are real, calling real endpoints (`src/features/notifications/api/notifications.queries.ts:33-48,58-75`). **There is genuinely no dedicated notifications list page yet** — this matches "cannot show" for Notifications directly.
- Warehouses/Inventory/Stock-Transactions pages (`WarehousesListPage.tsx`, `StockPage.tsx`, `StockTransactionsPage.tsx`, `WarehouseDetailPage.tsx`, `InventoryDetailPage.tsx`, `WarehouseCreateDialog.tsx`, `StockActionDialog.tsx`) **are real**, calling real `src/services/inventory/*` functions via TanStack Query (`src/features/inventory/api/inventory.queries.ts:39-191`) — Create and single-record fetch genuinely work against the backend.

**But list/search for these three is sourced from browser localStorage, not the backend**, because the backend has no list/search endpoint for Warehouse or Inventory at all (confirmed by the code's own comments):

- `inventory.queries.ts:47-50` — "No warehouse list endpoint exists... fetches the real `GetWarehouse` for each id created from this browser."
- `inventory.queries.ts:90-95` — same pattern for inventory records.
- Backed by `src/shared/stores/local-warehouses.store.ts:9-16` / `src/shared/stores/local-inventory-ids.store.ts:9-18` — Zustand `persist` stores keyed to **this browser's localStorage only**.

**Practical effect, and the likely explanation for "cannot show/search":** a warehouse or inventory record created in a different browser/session, or before this browser's localStorage was populated (or after it was cleared), simply won't appear in these lists — even though the record genuinely exists on the backend and `create`/open-by-id both work. This is a pre-existing, already-documented tradeoff (`docs/backlog.md` §2, `docs/roadmap.md` Phase 9), not a new regression — but it does produce exactly the reported symptom if testing happened in a fresh/cleared browser, or across two browser sessions.

## Separate, possibly-unrelated but worth fixing regardless: empty env vars

`.env.local` (repo root) currently sets all three of `NEXT_PUBLIC_API_BASE_URL`, `NEXT_PUBLIC_SIGNALR_HUB_URL`, `NEXT_PUBLIC_USE_MOCK_API` to **empty string** (not unset). `src/shared/lib/env.ts:1-11` uses `??`, which only falls back on `null`/`undefined`, not on `''` — so:

- `signalrHubUrl` resolves to `''` instead of the intended default `/hubs/global` (`realtime/signalr-client.ts:15`) — SignalR silently fails to connect (the failure is swallowed, `AuthGuard.tsx:36`: `connect().catch(() => undefined)`), affecting real-time notification push (`useNotificationRealtimeUpdates`).
- `useMockApi` resolves to `false` instead of the intended dev default `true` — the mock adapter never installs even in development (`src/shared/lib/api/mock.ts:14`).
- `apiBaseUrl` resolves to `''`, meaning every API call is relative (e.g. `/api/inventory/...`) and depends entirely on `next.config.ts`'s dev-only rewrite (`/api/:path*` → `http://localhost:5000/api/:path*`) actually reaching a live backend at that address — there's no other fallback anywhere in the repo.

**This affects the whole app equally, not just these four pages** — so it's likely not the primary explanation for a symptom scoped to only Inventory/Warehouse/Notification, but it's a real, silent misconfiguration worth fixing regardless (either delete the three empty lines from `.env.local` so the coded defaults apply, or set them to real values, and confirm `localhost:5000` is actually where the backend is reachable in this environment).

## Recommended fix directions

1. **Notifications:** build the real `/notifications` history page — pure frontend, no backend blocker, already an open item in `docs/roadmap.md` Phase 10 / `docs/backlog.md` §5. `shared/notifications/` components were built generically enough to support this without rework, per the roadmap's own note.
2. **Warehouse/Inventory/Stock-Transaction lists:** the lasting fix needs backend list/search endpoints (removes the local-tracking workaround entirely) — flag as a backend request if the local-tracking limitation is unacceptable; otherwise this is expected/documented behavior, not a bug.
3. **Env vars:** fix `.env.local` immediately regardless of the above — zero risk, currently silently breaking SignalR and forcing every dev-mode request through the network instead of mocks.

## Implementation

**Item 3 (env vars) — fixed.** Deleted `.env.local` (it's gitignored, not tracked, only had the three empty-string overrides) so `src/shared/lib/env.ts`'s coded defaults apply: `signalrHubUrl` → `/hubs/global`, `useMockApi` → `true` in dev, `apiBaseUrl` → `''` (unchanged, relies on the dev rewrite either way).

**Item 1 (notifications history page) — fixed.**

1. Added an optional `className` prop to `NotificationList.tsx` (merged via `cn`) so callers can override the dropdown's `max-h-96 overflow-y-auto` — needed for a full-page list that should scroll with the page, not a fixed box.
2. Built `NotificationsHistoryPage.tsx` (`src/features/notifications/components/`), mirroring `NotificationDropdown.tsx`'s structure (same `useNotificationsListQuery`/`useMarkAsReadMutation`/`NotificationDetailDialog`) but wrapped in `EntityHeader` with a "mark all read" action, `groupByCategory` on, and `className="max-h-none overflow-visible"`.
3. Replaced `(admin)/notifications/page.tsx`'s `PlaceholderModulePage` with the real component.
4. Updated `docs/backlog.md` §5 and `docs/modules/notification-center.md` to remove the now-resolved "still a placeholder" notes.

No new realtime subscription needed — `NotificationBell`'s `useNotificationRealtimeUpdates()` already runs globally in the admin layout.

Verified: `tsc --noEmit` and `eslint` both clean.

**Item 2 (real Warehouse/Inventory/Stock-Transaction lists) — fixed.** `Inventory.API` has `POST /warehouses/search`, `POST /inventories/search`, `POST /inventory-transactions/search` (all `RequireAdmin`, same `BuildingBlock.Criteria` keyword/filters/sorts/page/pageSize shape as `/users/search`). Frontend switch-over: added `searchWarehouses`/`searchInventories`/`searchInventoryTransactions` client functions under `src/services/inventory/`; `inventory.queries.ts` gained `useWarehousesSearchQuery`/`useInventoriesSearchQuery`/`useInventoryTransactionsSearchQuery`, all real paginated queries against the new endpoints; `WarehousesListPage.tsx`/`StockPage.tsx`/`StockTransactionsPage.tsx` now consume these instead of the local-id fan-out. `local-warehouses.store.ts`/`local-inventory-ids.store.ts` deleted since nothing reads from them anymore. Searchable fields: warehouses (`code`, `name`, `address`, `status`, `createdAt`); inventories (`productId`, `productVariationId`, `warehouseId`, `quantity`, `createdAt` — no keyword search); inventory-transactions (`inventoryId`, `productId`, `productVariationId`, `warehouseId`, `type`, `quantity`, `quantityAfter`, `reason`, `createdAt`). Verified: `tsc --noEmit`/`eslint` clean. Full detail: SimpleShop `docs/tasks/2026-07-22/Task5_warehouse-inventory-list-search-endpoints.md`.
