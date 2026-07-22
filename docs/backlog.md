# Backlog

**Purpose:** A structured, single place for every piece of known technical debt, deferred work, and "revisit when the backend changes" item across the whole project — so none of it lives only as a scattered code comment. Compiled during Phase 8 (Production Readiness) by reviewing every module doc, every `TODO` in source, and the codebase as a whole.

**Scope:** Frontend technical debt and deferred work only. Backend contract gaps are tracked primarily in [backend/coverage-report.md](./backend/coverage-report.md) and each module's own doc — this file cross-references them rather than duplicating the detail, and focuses on what the _frontend_ should do when they're resolved.

**Related documents:** [roadmap.md](./roadmap.md), [backend/coverage-report.md](./backend/coverage-report.md), every `docs/modules/*.md`.

**When to read:** Planning what to work on next, or before starting any new phase — check whether it resolves something here first.

**When to ignore:** Never — this is the canonical "what's left" list.

---

## How to use this file

Each item lists: what it is, why it's deferred rather than done, and what to do when it's no longer blocked. Items are grouped by category, not by phase. When an item is resolved, delete it from here and note the resolution in [changelog/CHANGELOG.md](./changelog/CHANGELOG.md) — don't just leave it checked off.

## 1. No automated test coverage — the single biggest gap versus "production-ready"

**What:** Zero test files exist anywhere in this project (`*.test.*`/`*.spec.*` — none). No test runner is installed (no Vitest/Jest/Playwright in `package.json`).

**Why deferred:** Every prior phase's brief scoped work as feature delivery against a real (but often gap-ridden) backend contract, with `yarn typecheck`/`yarn lint`/`yarn build` as the stated verification bar — never test authoring. Introducing a test framework from scratch is a meaningful toolchain/architecture decision in its own right (choice of runner, mocking strategy for the Axios/TanStack Query/Zustand stack, CI wiring), which Phase 8's "no major architectural changes" instruction argues against doing unilaterally here.

**What to do:** Before this app can honestly be called production-ready, add:

- Unit tests for pure logic that already exists and is easy to regress silently: `categories.utils.ts`'s `buildCategoryTree`/`flattenTree`/`collectDescendantIds`, the numeric-string Zod schemas (`products.schema.ts`, `inventory.schema.ts`), `useCursorList`'s page-param logic.
- Integration tests (React Testing Library) for the highest-risk interaction flows: the refresh-token queuing behavior in `shared/lib/api/client.ts`, cart add/remove/quantity logic, the local-tracking stores (`local-orders`/`local-warehouses`/`local-inventory-ids`/`notifications`' `readIds`).
- E2E smoke tests (Playwright) for the golden paths once a real backend environment exists: login → browse → add to cart → checkout → view order; create warehouse → stock-in → view transaction.
- This needs its own scoping/ADR before starting, given the toolchain decision involved.

## 2. Backend contract gaps carried as opaque/placeholder values

All of the following are backend limitations, not frontend bugs — resolving them requires backend changes, at which point the listed frontend file is the only place to update:

| Gap                                                                    | Frontend file(s) to revisit                                                                                                                                                                            | Backend doc                                                          |
| ---------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------- |
| `WarehouseStatus`/`InventoryTransactionType` have no published mapping | `shared/inventory/StockStatusBadge.tsx`                                                                                                                                                                | [backend/inventory/README.md](./backend/inventory/README.md)         |
| `ProductCategoryStatus` has no published mapping                       | `src/services/product/types/product-category-status.ts` (currently unused in UI — revisit if a status badge is ever added for categories)                                                              | [backend/product/README.md](./backend/product/README.md)             |
| `AuditAction` has no published mapping                                 | `src/services/audit/types/audit-action.ts` (Audit module itself isn't built yet — Phase 11 remains open, see §5)                                                                                       | [backend/audit/README.md](./backend/audit/README.md)                 |
| 12 Notification enums have no published mapping                        | `shared/notifications/*`, `features/notifications/*` — none currently render a status label, only opaque data                                                                                          | [backend/notification/README.md](./backend/notification/README.md)   |
| Order/Warehouse/Inventory have no list endpoints                       | `shared/stores/local-orders.store.ts`, `local-warehouses.store.ts`, `local-inventory-ids.store.ts` — replace with real paginated queries once list endpoints exist                                     | [backend/coverage-report.md](./backend/coverage-report.md)           |
| User service has no list/search/delete                                 | `features/users` — build the real table once these land                                                                                                                                                | [modules/user-management.md](./modules/user-management.md)           |
| No SignalR event catalog published by any of the 7 services            | `features/orders/hooks/useOrderRealtimeUpdates.ts` (`'OrderStatusChanged'`), `features/notifications/hooks/useNotificationRealtimeUpdates.ts` (`'NotificationCreated'`) — both placeholder event names | [realtime/signalr-strategy.md](./realtime/signalr-strategy.md)       |
| No unread-count endpoint / no read signal on the notification list DTO | `shared/stores/notifications.store.ts`'s `readIds` workaround                                                                                                                                          | [modules/notification-center.md](./modules/notification-center.md)   |
| No cross-service "get user by id" endpoint                             | `shared/commerce/OrderCustomer.tsx` (shows raw id only)                                                                                                                                                | [modules/order-management.md](./modules/order-management.md)         |
| Warehouse has no Update/Delete endpoint at all                         | `features/inventory/components/WarehousesListPage.tsx` (buttons disabled with tooltip)                                                                                                                 | [modules/inventory-management.md](./modules/inventory-management.md) |
| No stock-transfer endpoint                                             | `features/inventory/components/InventoryDetailPage.tsx` (Transfer button disabled with tooltip)                                                                                                        | [modules/inventory-management.md](./modules/inventory-management.md) |

## 3. Performance — considered, not changed (no measured need yet)

- **`@microsoft/signalr` is a static top-level import** in `shared/lib/realtime/signalr-client.ts`. It's only ever imported (transitively) by `(admin)`-route code (`AuthGuard`, the two realtime feature hooks), so Next.js's route-based chunking likely already keeps it out of the public `(auth)` bundle — but this hasn't been confirmed with an actual bundle analysis. If a future bundle-size audit shows it's meaningfully inflating the admin chunk, converting `getConnection()` to lazily `import('@microsoft/signalr')` is the fix — deferred rather than done now because it'd turn the currently-synchronous `on`/`off` API async, a real correctness risk (event handler registration/cleanup ordering) not worth taking without evidence it matters.
- **Column definitions in list pages** (`ProductsListPage`, `OrdersListPage`, `WarehousesListPage`, etc.) are plain `const columns: ColumnDef<T>[] = [...]` recomputed every render, rather than `useMemo`'d. `AppDataTable` itself memoizes on top of whatever it receives, so this only matters if a parent re-renders very frequently — not observed or measured to be a problem at this app's data scale (10–20 rows/page). Left alone per "do not optimize prematurely, focus on measurable improvements."
- No React DevTools Profiler / Lighthouse run was performed this phase (no browser available in this environment) — a real performance pass needs one before further optimization is justified.

## 4. Accessibility — structurally sound, not independently verified

Every icon-only button in this app requires an `aria-label` at the TypeScript level (`IconButton`'s prop type), dialogs/popovers/dropdowns/tabs/selects all come from Radix primitives (focus trapping, `Escape`-to-close, and ARIA roles are handled by the library, not hand-rolled), and forms wire `aria-invalid`/`aria-describedby` (`shared/forms/FormField.tsx`, added Phase 2). Phase 8 fixed several **hardcoded English `aria-label`/dialog-title strings** found during review (see [changelog/CHANGELOG.md](./changelog/CHANGELOG.md)'s Phase 8 entry) — those were real defects, not just theoretical.

What's still open:

- **No automated accessibility testing** (axe-core, Lighthouse CI, or similar) has ever been run against this app — everything above is "structurally should be fine" reasoning, not a verified WCAG pass. No browser was available in any session this project was built in.
- **Color contrast** relies entirely on shadcn/Tailwind's default theme tokens — never independently checked against WCAG AA with a real contrast checker.
- `use-mobile.ts` (`src/shared/hooks/use-mobile.ts`) is the one file in this codebase that breaks the documented `useXxx.ts` hook-naming convention ([conventions/naming.md](./conventions/naming.md)) — it's shadcn CLI-generated (vendored alongside `src/components/ui/sidebar.tsx`, which imports it), not hand-authored, so it was left as-is rather than renamed (renaming vendored/generated code risks silently breaking future `shadcn add` regenerations). Documented here and in `conventions/naming.md` as a deliberate, explained exception rather than an unexplained inconsistency.

## 5. Modules not yet built (by design — future roadmap phases)

- **Audit** (roadmap Phase 11) — a real audit-log viewer page was never built; `shared/entity/AuditTrailButton`/`AuditTrailDialog` only ever fetch a bounded recent window client-filtered by entity id (see [backend/audit/README.md](./backend/audit/README.md) — `ListAuditLogs` has no server-side entity-id filter). A dedicated `/audit` page with real search/filter/pagination against the full log is still open.
- **Notification Campaigns/Rules/Groups/Channels** — reserved nav placeholders only (`implemented: false`), per every phase's explicit instruction not to build these yet, even though their backend endpoints already exist (confirmed in [backend/notification/README.md](./backend/notification/README.md)).
- **A full-page `/notifications` history view** — the nav route exists and is still `PlaceholderModulePage`; only the topbar bell/dropdown/detail experience was built (see [modules/notification-center.md](./modules/notification-center.md)). `shared/notifications`'s components were built generically enough to support this without rework.
- **Stock Transfer** — no backend endpoint exists; `InventoryDetailPage` has a disabled placeholder button ready for when one does.
- **Admin "create order for customer"** — the backend endpoint (`POST /orders/admin`, `adminCreateOrder()` in `src/services/order/`) exists and requires `Admin`/`Root`, but no admin-facing page/form was built for it — the same "reserve, don't implement" treatment given to Notification Campaigns/Rules/Groups/Channels above. See [backend/order/README.md](./backend/order/README.md) and [modules/client-mock.md](./modules/client-mock.md).

## 6. Tooling gaps

- **`eslint-plugin-boundaries`** (or equivalent) was never wired in — the `app → features → shared` dependency direction and the "no deep cross-feature imports" rule are enforced by hand (grep-based checks at the end of every phase in this project's history) rather than automatically. Flagged as a TODO since [conventions/imports-and-boundaries.md](./conventions/imports-and-boundaries.md) was written in Phase 0 and still true.
- **i18n is structurally multi-locale-ready but only ships `en`** — every string lives in a namespaced JSON file and is registered in `src/i18n/request.ts`; adding a second locale means adding a `messages/<locale>/` set and registering it, no component refactoring required (confirmed true by this phase's review — this is a genuinely met goal, not a gap). `src/app/layout.tsx`'s `<html lang="en">` is currently hardcoded, though — it should read from `locale.store.ts`/next-intl's resolved locale once a second locale actually ships.
