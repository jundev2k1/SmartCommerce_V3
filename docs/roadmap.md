# Roadmap

**Purpose:** Master guide for implementation order across the whole project lifetime. The single source of truth for "what phase are we in and what does it require."

**Scope:** Phase sequencing, dependencies, and completion criteria. Does not contain architectural rationale (see [decisions/](./decisions/template.md)) or naming rules (see [conventions/](./conventions/naming.md)).

**Related documents:** [README.md](./README.md), [modules/overview.md](./modules/overview.md), [architecture/overview.md](./architecture/overview.md)

**When to read:** Start of every session, to confirm which phase is active and which docs that phase requires.

**When to ignore:** Never — always check this first to know what to build next.

---

## How to use this roadmap

Each phase is independently executable and produces a working, runnable application increment. Do not start a phase until its dependencies are complete. Each phase entry tells you exactly which docs to load — do not read docs outside that list unless you hit an open question.

---

## Phase 0 — Foundation & Documentation

- **Goals:** Establish architecture, conventions, doc system, and roadmap. No business code.
- **Dependencies:** None.
- **Expected outputs:** Complete `/docs` tree (this phase's deliverable).
- **Docs required:** All of them (this is where they're written).
- **Docs to ignore:** N/A.
- **Implementation order:** N/A — documentation only.
- **Completion criteria:** `/docs` tree exists per [README.md](./README.md), every doc has a header block, every decision in [decisions/](./decisions/template.md) is recorded.

## Phase 1 — Scaffold & Core Infrastructure

- **Goals:** Runnable Next.js shell with theming, i18n, API client, and state plumbing in place — no business features yet.
- **Dependencies:** Phase 0 complete.
- **Expected outputs:** `yarn dev` boots a themed, empty admin shell. `src/app`, `src/features` (empty), `src/shared/{ui,forms,lib,stores,config}` scaffolded. Axios client + TanStack Query provider wired against a mock adapter. Zustand store skeleton (theme, session placeholder). next-intl configured with `en` messages. next-themes wired (dark/light/system).
- **Documents required:** [architecture/overview.md](./architecture/overview.md), [architecture/folder-structure.md](./architecture/folder-structure.md), [architecture/tech-stack.md](./architecture/tech-stack.md), [conventions/naming.md](./conventions/naming.md), [conventions/imports-and-boundaries.md](./conventions/imports-and-boundaries.md), [frontend/ui-components.md](./frontend/ui-components.md), [frontend/theming.md](./frontend/theming.md), [frontend/i18n.md](./frontend/i18n.md), [state/overview.md](./state/overview.md), [state/zustand-strategy.md](./state/zustand-strategy.md), [services/api-layer.md](./services/api-layer.md), [realtime/signalr-strategy.md](./realtime/signalr-strategy.md) (manager is scaffolded now, not connected), [modules/overview.md](./modules/overview.md) (bounded-context nav grouping), all [decisions/](./decisions/template.md).
- **Documents to ignore:** [api/](./api/README.md) (no backend yet).
- **Implementation order:** Next.js init (yarn, TS, Tailwind, src/ dir) → shadcn/ui install → `shared/ui` wrapper layer → next-themes → next-intl → TanStack Query provider → Axios client + mock adapter → Zustand store skeleton → empty `features/` dir with its own README.
- **Completion criteria:** App boots, theme toggle works, one mock API call round-trips through the Axios client into a TanStack Query hook, no lint/type errors.

## Phase 1.5 — Backend Contract Integration (Swagger)

- **Goals:** Integrate the real backend OpenAPI contracts (audit, auth, inventory, notification, order, product, user) into the frontend architecture — typed request/response DTOs, one-file-per-endpoint service functions, and documentation. No UI, no business pages, no feature hooks.
- **Dependencies:** Phase 1.
- **Expected outputs:** `src/services/<service>/` for all 7 services (69 endpoints total); `docs/backend/` (per-service docs, feature mapping, coverage report); corrected `shared/lib/api/{client,types,env}.ts` to match the real response envelope (Phase 1 had guessed wrong — see [decisions/0012-backend-service-client-layer.md](./decisions/0012-backend-service-client-layer.md)).
- **Documents required:** [services/api-layer.md](./services/api-layer.md), [conventions/naming.md](./conventions/naming.md), [decisions/0005-api-layer-and-auth.md](./decisions/0005-api-layer-and-auth.md), [decisions/0012-backend-service-client-layer.md](./decisions/0012-backend-service-client-layer.md), [state/query-strategy.md](./state/query-strategy.md), [modules/overview.md](./modules/overview.md).
- **Documents to ignore:** UI-focused docs ([frontend/](./frontend/routing.md)) — nothing here touches components or pages.
- **Implementation order:** Fix shared Axios infra for the real envelope → build `src/services/shared/paginated-result.ts` → build each service smallest-first (audit → auth → user → order → inventory → product → notification) → write `docs/backend/`.
- **Completion criteria:** `yarn typecheck`/`yarn lint` clean; every endpoint in all 7 Swagger files has a matching function in `src/services/`; [backend/coverage-report.md](./backend/coverage-report.md) accounts for every endpoint plus every gap/ambiguity found.

## Phase 2 — Authentication & Identity (IAM) ✅ Complete

- **Goals:** Login/logout against the real Auth contract, HTTP-only cookie session flow, refresh-token interceptor, route guards, and — pulled forward from Phase 4 — the first real admin module (User Management).
- **Dependencies:** Phase 1.5.
- **Actual outputs:** `features/auth` (`AuthGuard`/`GuestGuard`, login/register pages+forms, session query, logout) and `features/users` (create/edit against the real contract; list/delete placeholder-and-TODO since the User service didn't expose a search endpoint at the time — see [modules/user-management.md](./modules/user-management.md) and [backend/coverage-report.md](./backend/coverage-report.md); list/search since resolved 2026-07-24, delete remains unavailable). Two small additive touches to already-built shared files: `onLogout` callback prop threaded through `shared/layout/{AdminShell,Topbar,UserMenu}.tsx` ([decisions/0013](./decisions/0013-shared-shell-callback-props.md)), and `aria-invalid`/`aria-describedby` added to `shared/forms/FormField.tsx`.
- **Documents:** [modules/authentication.md](./modules/authentication.md), [modules/user-management.md](./modules/user-management.md), [backend/auth/README.md](./backend/auth/README.md), [backend/user/README.md](./backend/user/README.md), [decisions/0013-shared-shell-callback-props.md](./decisions/0013-shared-shell-callback-props.md).
- **Completion criteria:** Login/register/logout work against the real contract; session restores on reload; `(admin)` routes redirect to `/login` when unauthenticated and `(auth)` routes redirect to `/` when already authenticated; Users page shows real create/edit against the backend with list/delete honestly marked unavailable rather than faked.

## Phase 3 — Admin Shell & Navigation ✅ Complete (built ahead of schedule, in Phase 1)

- **Note:** This phase's entire scope — `shared/config/nav.ts` grouped by bounded context, `AdminShell`/`Sidebar`/`Topbar`, disabled-state rendering for unimplemented modules — was already built as part of Phase 1's "professional admin dashboard layout" requirement, before this phase was reached in sequence. Recorded here for roadmap accuracy; no additional work was needed when this phase's turn came up.
- **Completion criteria (retroactively satisfied):** All modules from the module list appear in nav; unimplemented ones render disabled/"coming soon"; layout persists across route changes.

## Phase 4 — User Management → merged into Phase 2

- User Management was pulled forward and completed alongside Authentication in Phase 2 (see above) rather than as its own later phase — the two are tightly coupled (session restoration depends on the User service's `GetUserDetail`, and User Management was the natural "first CRUD module" to prove out Feature-First against a real contract). See [modules/user-management.md](./modules/user-management.md) for what's real vs. placeholder.

## Phase 5 — Product Management (Products, Variants, Categories, Tags) ✅ Complete

- **Actual outputs:** `features/products` (Products + Variants — no separate `features/product-variants`; variants have no independent list endpoint, so they're the "Variants" tab on a product's detail page, not their own nav entry — see [modules/product-management.md](./modules/product-management.md)), `features/categories` (hierarchical tree, arbitrary depth), `features/tags`. New shared layer `shared/entity/` extracted (see [decisions/0014-shared-entity-component-layer.md](./decisions/0014-shared-entity-component-layer.md)) once these four modules made the repeated header/toolbar/confirm-delete/audit-button pattern concrete.
- **Documents:** [backend/product/README.md](./backend/product/README.md), [modules/product-management.md](./modules/product-management.md), [modules/product-categories.md](./modules/product-categories.md), [modules/product-tags.md](./modules/product-tags.md), [decisions/0014-shared-entity-component-layer.md](./decisions/0014-shared-entity-component-layer.md).
- **Completion criteria:** Full CRUD against the real contract (Product's 25 endpoints have no gaps, unlike Users) — search/filter/sort/pagination/column-visibility/row-selection on the Products list; variant add/edit/delete/reorder/set-default; category tree with re-parenting; tag CRUD (no color/usage-count — confirmed unsupported, not faked).

## Phase 6 — Product Search ✅ Complete

- **Actual output:** `features/product-search` — a card-grid browse experience reusing `features/products`'s `useSearchProductsQuery`/`ProductFilters` (not a duplicate implementation), demonstrating "reusable by future Client pages" concretely rather than just asserting it. See [modules/product-search.md](./modules/product-search.md).
- **Completion criteria:** Search/filter/pagination works against the real `SearchProducts` contract.

## Phase 7 — Cart & Checkout ✅ Complete (delivered as part of Phase 4 "Client Mock")

- **Actual outputs:** `features/cart` (client-only cart via `shared/stores/cart.store.ts` — no cart API exists on any of the 7 services), `features/checkout` (`features/checkout/api/checkout.queries.ts`'s `useCreateOrderMutation` against the real `CreateOrder` contract). Checkout submits against the **real** Order service, not a mock — the "against mocks" framing in this phase's original goal was superseded once Phase 1.5 integrated the real contract.
- **Documents:** [modules/client-mock.md](./modules/client-mock.md), [decisions/0015-shared-commerce-component-layer.md](./decisions/0015-shared-commerce-component-layer.md).
- **Completion criteria:** Cart persists client-side (add/remove/update-quantity/clear); checkout builds a real `CreateOrderRequest` and submits it, with a documented limitation that the request schema has no variation-level field (see [modules/client-mock.md](./modules/client-mock.md)).

## Phase 8 — Orders & Order History ✅ Complete (delivered as part of Phase 4 "Client Mock"; upgraded in Phase 6 "Sales")

- **Actual outputs (Phase 4):** `features/orders` — Order Detail against the real `GetOrder` contract; "My Orders" list resolved the documented list-endpoint gap by tracking order ids created from the current browser (`shared/stores/local-orders.store.ts`) and fetching each via the real `GetOrder`, rather than waiting on a backend list endpoint or leaving it a bare placeholder — see [modules/client-mock.md](./modules/client-mock.md) for the tradeoffs. Cancel is wired to the real `CancelOrder` endpoint.
- **Actual outputs (Phase 6 "Sales" upgrade):** Same `features/orders`, elevated into the permanent admin Order Management module rather than duplicated into a new `features/sales` — richer List (customer/date-range filters, sort, refresh, bulk-selection placeholder), a sectioned Detail page (`OrderSummaryCard`/`OrderItems`/`OrderCustomer`/`OrderTimeline`/`OrderActions`/`OrderMetadata` in `shared/commerce`), and realtime updates via a new reusable `shared/lib/realtime/useHubEvent.ts` abstraction + `features/orders/hooks/useOrderRealtimeUpdates.ts` (placeholder `'OrderStatusChanged'` event, targeted per-order cache invalidation). SignalR connection lifecycle (`connect()`/`disconnect()`) wired to auth session for the first time, ahead of Phase 10. See [modules/order-management.md](./modules/order-management.md).
- **Documents:** [backend/order/README.md](./backend/order/README.md), [modules/client-mock.md](./modules/client-mock.md), [modules/order-management.md](./modules/order-management.md), [realtime/signalr-strategy.md](./realtime/signalr-strategy.md).
- **Completion criteria:** Order detail view against the real contract; order list sourced from locally-tracked ids (documented workaround) since no backend list endpoint exists; realtime updates integrated against a placeholder event pending a published SignalR event catalog.

## Phase 9 — Inventory, Warehouses, Stock Transactions ✅ Complete

- **Actual outputs:** `features/inventory` — Warehouses (Create + Detail real; Update/Delete disabled placeholders since neither endpoint exists), Stock (a product-scoped rollup lookup via `GetProductStock`, plus a paginated inventory-record table), Stock Transactions (paginated transaction search). New shared layer `shared/inventory/` (ADR 0016). **Update 2026-07-24:** List views originally shipped via browser-scoped local-id-tracking (Create/GetById existing for both Warehouse and Inventory made that a reasonable stopgap); since superseded by real `POST /warehouses/search`, `/inventories/search`, `/inventory-transactions/search` endpoints — `local-warehouses.store.ts`/`local-inventory-ids.store.ts` deleted, list/search/filter is now real server-side pagination (see `docs/tasks/2026-07-22/Task7_inventory-warehouse-notification-integration-gaps.md`).
- **Documents:** [backend/inventory/README.md](./backend/inventory/README.md), [modules/inventory-management.md](./modules/inventory-management.md), [decisions/0016-shared-inventory-component-layer.md](./decisions/0016-shared-inventory-component-layer.md).
- **Completion criteria:** Inventory/warehouse/stock-transaction detail + write flows (stock-in/out/adjust) against the real contract — done; list/search now real server-side pagination as of 2026-07-24 (see Update above), no longer a local-tracking workaround.

## Phase 10 — Notifications (in-app + SignalR realtime) ✅ Complete (delivered as Phase 7 "Notification Center")

- **Actual outputs:** `features/notifications` (`NotificationBell`, `NotificationDropdown`, `NotificationDetailDialog`) + a new presentational layer `shared/notifications/` (ADR 0017) + a new generic `shared/hooks/useCursorList.ts`. Reused, unmodified, the SignalR connection lifecycle and `useHubEvent` hook Phase 6 ("Sales") wired ahead of schedule — this phase only added the notification-specific event subscription (`'NotificationCreated'`, placeholder) and UI. Campaign/Rule/Group/Channel admin screens remain exactly the disabled nav placeholders Phase 1 already created — untouched this phase, confirming "reserve, don't implement" held.
- **Two real contract gaps shaped the whole module** (see [modules/notification-center.md](./modules/notification-center.md)): `ListMyUserNotifications` has no opaque cursor token, only page/pageSize — `useCursorList` drives `useInfiniteQuery` with a page number as `pageParam` instead, still delivering real infinite-scroll UX. And there's no unread signal on the list DTO at all (`NotificationStatus` is one of the 12 opaque unpublished enums, and `readAt` only exists on the single-detail response) and no unread-count endpoint — unread is computed client-side from a persisted `readIds` set (`shared/stores/notifications.store.ts`), scoped to what's loaded into this browser, not a true historical count.
- **Documents:** [backend/notification/README.md](./backend/notification/README.md), [modules/notification-center.md](./modules/notification-center.md), [realtime/signalr-strategy.md](./realtime/signalr-strategy.md), [decisions/0017-shared-notifications-component-layer.md](./decisions/0017-shared-notifications-component-layer.md).
- **Completion criteria:** Notification bell shows realtime events (placeholder event, real cache-update pattern); infinite-scroll history list works against the real contract (page-number-driven, not a true cursor); Campaign/Rule/Group/Channel remain disabled nav entries despite their endpoints existing.

## Phase 11 — Audit

- **Goals:** Read-only audit log viewer.
- **Dependencies:** Phase 3.
- **Documents required:** [backend/audit/README.md](./backend/audit/README.md), [state/query-strategy.md](./state/query-strategy.md), [decisions/0010-pagination-and-cursor-strategy.md](./decisions/0010-pagination-and-cursor-strategy.md).
- **Completion criteria:** Audit log list + filters against the real contract, no write path.

## Phase 12 — Backend Integration Pass

- **Goals:** Point every `src/services/<service>` client at a live backend environment (contracts themselves were already integrated in Phase 1.5) and remove any mock adapters still used during feature development.
- **Dependencies:** All prior phases; a reachable live/staging backend.
- **Documents required:** [backend/README.md](./backend/README.md), [backend/coverage-report.md](./backend/coverage-report.md) (resolve any endpoints that were missing during feature development), [services/api-layer.md](./services/api-layer.md), [services/error-handling.md](./services/error-handling.md).
- **Completion criteria:** `NEXT_PUBLIC_API_BASE_URL` points at a real gateway; no mock adapter (`axios-mock-adapter`) remains wired into any production code path; every gap noted in [backend/coverage-report.md](./backend/coverage-report.md) is either resolved or explicitly still open.

## Production Readiness, Optimization & Final Polish ✅ Complete

Not a numbered roadmap phase — a cross-cutting consistency/quality review pass across every module built so far (Phases 1 through the Phase-7-equivalent Notification Center work above), rather than new feature delivery. Explicitly did **not** touch Phase 11 (Audit) or Phase 12 (Backend Integration Pass) — both remain genuinely open, tracked in [backlog.md](./backlog.md), not silently marked done.

- **Fixed real defects found during review**: a leftover `console.log(env)` in `shared/lib/api/client.ts`; a dead `readEnv` helper in `shared/lib/env.ts` (now actually used); hardcoded English `aria-label`s and dialog-title strings across `features/products`, `features/categories`, `features/tags`, `features/orders`, and even inside `shared/entity`/`shared/commerce`/`shared/ui` components that had bypassed i18n entirely (added `entity.confirmDelete`-keyed titles, `common.actions` keys, and optional-with-English-default props on `AppSearchBox`/`AppDataTable` for the two shared-layer components that have no i18n dependency of their own by design).
- **Extracted one proven duplicated pattern**: `shared/entity/EntityDetailHeader` (breadcrumb + `EntityHeader`), after finding the exact same composition repeated verbatim across five detail pages — see [decisions/0014](./decisions/0014-shared-entity-component-layer.md)'s Phase 8 update.
- **Reviewed, did not change**: several performance ideas (lazy-loading `@microsoft/signalr`, memoizing list-page column arrays) were considered and explicitly deferred rather than applied without measurement data — see [backlog.md](./backlog.md) §3.
- **Compiled `docs/backlog.md`**: every backend-contract-gap workaround, every reserved-for-later module, every tooling gap (no test suite, no `eslint-plugin-boundaries`, no automated a11y testing), and one documented naming-convention exception (`use-mobile.ts`, vendored shadcn CLI output), all in one place instead of scattered across code comments.
- **Documents:** [backlog.md](./backlog.md), [conventions/naming.md](./conventions/naming.md), [decisions/0014-shared-entity-component-layer.md](./decisions/0014-shared-entity-component-layer.md), [frontend/ui-components.md](./frontend/ui-components.md).
- **Completion criteria:** `yarn typecheck`/`yarn lint`/`yarn build` all clean; every module doc reviewed and cross-referenced; no undocumented technical debt remains — everything found is either fixed or listed in `backlog.md`.

---

## Notes for future sessions

- Do not author a `docs/modules/<name>.md` until that module's phase actually starts — copy [modules/_template.md](./modules/_template.md) at that time.
- Do not author real content in [api/](./api/README.md) until backend OpenAPI is actually provided — do not guess API contracts.
- If a phase's scope grows too large mid-implementation, split it (e.g. 5a/5b/5c) rather than letting one phase balloon — log the split in [changelog/CHANGELOG.md](./changelog/CHANGELOG.md).
- Check [backlog.md](./backlog.md) before starting any new phase — it's the canonical "what's left" list, kept current since Phase 8.
