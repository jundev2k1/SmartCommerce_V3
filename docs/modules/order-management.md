# Order Management (Sales)

**Purpose:** The Sales bounded context's admin-facing Order Management experience — Order List, Order Detail, realtime status updates — connecting Catalog (product/variant references), Inventory (independently, no direct coupling), and the shared SignalR infrastructure.

**Scope:** `features/orders` (upgraded from Phase 4's simpler "My Orders" mock) and the `(admin)/orders`, `(admin)/orders/[orderId]`, `(admin)/orders/approve`, `(admin)/orders/history` routes, plus the Order-related additions to `shared/commerce` and the new `shared/lib/realtime/useHubEvent.ts` abstraction.

**Related documents:** [modules/overview.md](./overview.md), [modules/client-mock.md](./client-mock.md), [backend/order/README.md](../backend/order/README.md), [realtime/signalr-strategy.md](../realtime/signalr-strategy.md), [decisions/0007-signalr-strategy.md](../decisions/0007-signalr-strategy.md), [decisions/0015-shared-commerce-component-layer.md](../decisions/0015-shared-commerce-component-layer.md).

**When to read:** Touching Order List/Detail, the Order-related `shared/commerce` components, or the SignalR subscription pattern.

**When to ignore:** Cart/Checkout/Shop (still [client-mock.md](./client-mock.md)) or any other bounded context.

---

## Relationship to Phase 4's "Client Mock" Orders

Phase 4 built `features/orders` framed as a customer's "My Orders" (part of a temporary customer-flow mock). This phase elevates the same feature folder into the permanent, admin-facing Order Management module — there is one admin shell/one session for both roles, so rather than duplicate a second `features/sales` against the identical `GetOrder`/`CancelOrder` contract, this phase **extends `features/orders` in place**: richer filters/sort on the list, reusable sectioned components on the detail page, and realtime updates. [client-mock.md](./client-mock.md) still documents the Cart/Checkout submission flow that feeds orders into this module; this doc covers everything downstream of that.

A later change (see "Admin Approve List and customer Order History" below) added real role-based visibility on top of that one shell: `/orders` (this full admin table) and `/orders/approve` are hidden from and blocked for the `User` role, while `/orders/[orderId]` (detail) and the new `/orders/history` stay reachable by both roles. It's still one shell/one session — the split is enforced by nav filtering + route guarding, not by a separate app or layout.

## Feature folder(s)

`src/features/orders/`. Order-specific shared presentation lives in `src/shared/commerce/` (`OrderSummaryCard`, `OrderItems`, `OrderCustomer`, `OrderTimeline`, `OrderStatusBadge`, `OrderActions`, `OrderMetadata`) — the same layer Phase 4 introduced, extended per [decisions/0015](../decisions/0015-shared-commerce-component-layer.md).

## The contract still has no list endpoint — this doesn't change in this phase

Everything in [client-mock.md](./client-mock.md)'s "real contract gaps" section still applies: `GetOrder`/`CreateOrder`/`CancelOrder` are the entire Order contract, so Order List remains sourced from `shared/stores/local-orders.store.ts` (ids of orders created from this browser). **This is a real limitation for an "admin manages all orders" framing** — an admin who didn't personally place orders through this browser's Checkout won't see other customers' orders. There is no way around this without a backend list endpoint; it's called out prominently here (rather than silently inherited from Phase 4) because this phase's brief specifically asks for admin order _management_, which implies seeing orders beyond your own. What this phase adds — search/filter/sort/pagination/refresh/bulk-selection-placeholder/realtime — is real and fully functional over whatever set of orders is known, it's just scoped by the same underlying constraint.

## Order List

`OrdersListPage` — search (order id), filters (status, customer id substring, created-date range), client-side sort (created date / total, asc/desc), pagination, a Refresh button (`queryClient.invalidateQueries({ queryKey: orderKeys.details() })` — refetches every currently-known order), and a bulk-selection placeholder (`AppDataTable`'s row-selection + `shared/entity`'s `SelectionPanel`, whose bulk actions render disabled since no bulk order endpoint exists, per the same policy as every other module's `SelectionPanel` usage).

## Order Detail

Reorganized into independent tab sections so future additions don't require redesigning the page:

| Section           | Component                   | Notes                                                                                       |
| ----------------- | --------------------------- | ------------------------------------------------------------------------------------------- |
| Order Information | `OrderSummaryCard`          | id/status/total/created-at                                                                  |
| Products          | `OrderItems`                | line items — no variant or discount columns, since `GetOrderItemResponse` has neither field |
| Customer          | `OrderCustomer`             | id only — see below                                                                         |
| Timeline          | `OrderTimeline`             | current status + realtime-observed events, see below                                        |
| Metadata          | `OrderMetadata`             | thin wrapper over `shared/entity`'s `EntityMetadata`                                        |
| Actions           | `OrderActions` (header)     | Cancel only                                                                                 |
| Audit             | `AuditTrailButton` (header) | reused as-is, `service="Order"`                                                             |

**Payment Information and Shipping Information are omitted entirely** — neither `CreateOrderRequest` nor `GetOrderResponse` has any payment or shipping field at all, and the brief itself says "if available." Nothing was invented to fill these sections.

**Discounts are omitted** for the same reason — no discount field exists on `GetOrderItemResponse` or `GetOrderResponse`.

## Admin Approve List and customer Order History

Two more pages sourced from the same `useLocalOrdersQuery()` data every order view uses (same underlying limitation — see "The contract still has no list endpoint" above):

- **`OrderApproveListPage`** (`/orders/approve`, Admin-only) — filters to `status === OrderStatus.Confirmed`. "Approve" calls the real `CompleteOrder` endpoint (`POST /orders/{orderId}/complete`, admin-only on the backend too — `RequireAdmin`), moving the order to `Completed`; the row naturally drops out of the Confirmed filter afterward. "Cancel" reuses the existing `useCancelOrderMutation()` — no new mutation was written for it. **This is a genuinely new backend capability, not a pre-existing one**: `Order.Domain/Entities/Order.cs` already had a `Complete()` domain method, but no Application command/endpoint exposed it before this — `CompleteOrder.cs` (endpoint), `CompleteOrderCommand`/`CompleteOrderHandler`, and `OrderCompletedIntegrationEvent` were added to the Order service specifically to back this page, rather than building a button with no real backend effect. See `backend/order/README.md`. `OrderCompletedIntegrationEvent` has no consumer yet (Notification Service doesn't react to Completed orders) — flagged in `docs/services/order-service.md` on the backend side, not built here since no realtime/notification requirement was in scope.
- **`OrderHistoryListPage`** (`/orders/history`, unrestricted) — the customer-facing equivalent of `OrdersListPage`, same data source, rendered as an `OrderSummaryCard` grid instead of an admin data table (no search/filter/sort/bulk-selection — that admin tooling stays on `/orders`, which a `User`-role account can no longer reach, see below).

There is still no "Pending → admin manually approves → Confirmed" flow: `CreateOrder` persists `Pending` and the `CreateOrderSaga` auto-confirms it (`DeductInventoryStep → ConfirmOrderStep`) with no human in the loop, unchanged by this addition. "Approve" here means _fulfillment_ completion (Confirmed → Completed) — conceptually the step a future Shipper role would perform — not approval of the original order.

## Role-based access (Admin vs. User)

`/orders` (the full admin table) and `/orders/approve` require the `Admin` role — hidden from the sidebar and blocked at the route level (redirects to `/403`) for a `User`-role session, via `shared/config/nav.ts`'s `roles` field on `NavGroup`/`NavItem` and `AuthGuard.tsx`'s `getRequiredRolesForPath` check (see [modules/overview.md](./overview.md) for the general nav/role mechanism, shared across every admin-only module). `/orders/[orderId]` (detail) and `/orders/history` are reachable by any authenticated role — a customer needs to view/track their own orders — which is why `/orders` is deliberately excluded from the prefix-match rule that lets `/products/[id]`-style detail routes inherit their list page's restriction; see the comment on `PREFIX_MATCH_EXCLUDED_HREFS` in `nav.ts`.

## Customer Information: id only

`OrderCustomer` shows the raw `customerId` and nothing else. `GetOrderResponse` has no customer name/email, and there's no cross-service lookup available — the User service has no "get user by arbitrary id" endpoint, only `GetUserDetail` for the _current_ session's own profile (see [backend/user/README.md](../backend/user/README.md) and [modules/user-management.md](./user-management.md)'s own list/delete gap). Displaying a name here would require either a new backend endpoint or guessing — neither was done.

## Order Timeline: real backend history doesn't exist, so this shows what can genuinely be observed

`GetOrderResponse` has no status-history array (confirmed in Phase 4). This phase's `OrderTimeline` is upgraded from a pure placeholder into a component that also renders **realtime-observed events** — status changes pushed via SignalR while the Order Detail page happens to be open (see below). Before any such event arrives, it still shows only the current status with a plain note, exactly as Phase 4 did; it never backfills history that doesn't exist. The component itself is generic (`OrderTimelineEvent { status, timestamp, operator?, message? }`) so any future module with the same "status + optionally-observed live events" shape can reuse it, per the brief's "future modules should also be able to reuse it."

## SignalR integration

**No SignalR event catalog is published by any of the 7 backend services** — confirmed again this phase (the Notification service, the only one that could plausibly document push events, is REST-only; see [backend/notification/README.md](../backend/notification/README.md)). [realtime/signalr-strategy.md](../realtime/signalr-strategy.md) already anticipated this exact situation for Phase 10 ("mock/simulate events locally... until the real contract exists") — the same policy applies here, ahead of schedule.

What was actually built, real and working today regardless of the missing event catalog:

- **`shared/lib/realtime/useHubEvent.ts`** (new) — a generic "subscribe to one hub event for this component's lifetime" hook, so no feature ever imports `signalr-client` directly. Reusable by any future realtime feature (Notifications, Phase 10).
- **Connection lifecycle wired to auth**, per `docs/realtime/signalr-strategy.md`'s own stated design (previously nothing called `connect()`/`disconnect()` at all): `features/auth/components/AuthGuard.tsx` calls `connect()` once a session is confirmed; `features/auth/api/auth.queries.ts`'s `useLogoutMutation` calls `disconnect()` in `onSettled`. Both are fire-and-forget (`.catch(() => undefined)`) since a failed connection attempt (expected without a live backend/hub in this environment) must not break the auth flow.
- **`features/orders/hooks/useOrderRealtimeUpdates.ts`** — subscribes to a placeholder event name, `'OrderStatusChanged'`, clearly marked as unconfirmed. On receipt: invalidates only `orderKeys.detail(event.orderId)` — never a whole-list invalidation, since "the list" here is just a set of individual detail queries (`useLocalOrdersQuery`'s `useQueries`), so a targeted invalidation is already the minimal correct one and the list page picks up the change automatically. `OrdersListPage` calls this hook for the auto-refresh side effect; `OrderDetailPage` calls it with a callback that also appends the event to its local "observed events" list for `OrderTimeline`.

**When the backend publishes a real event catalog**, only `useOrderRealtimeUpdates.ts`'s event name and payload shape need to change — the subscription mechanism, invalidation strategy, and timeline rendering are already correct.

## API / State

Consumes only `src/services/order` via `features/orders/api/orders.queries.ts`'s existing query/mutation hooks (`useOrderQuery`, `useLocalOrdersQuery`, `useCancelOrderMutation`) — no new service-layer code was needed this phase. Server state stays in TanStack Query; the only Zustand involvement is the pre-existing `local-orders.store.ts` (which order ids to fetch) and `signalr.store.ts` (connection status) — no new UI-state store was needed.

## Audit

`AuditTrailButton`/`AuditTrailDialog` from `shared/entity`, `service="Order"` — no Order-specific audit implementation, per the brief's explicit instruction.

## Dependencies on other modules

References **Product Management** indirectly (order items carry `productId`/`productName` already denormalized onto `GetOrderItemResponse` by the backend, so no live Catalog lookup is needed here — a deliberate difference from Inventory's `InventorySummaryCard`, which does need a live Product lookup). Does **not** reference Inventory directly — Sales and Inventory are independent bounded contexts per the brief, and nothing in the Order contract exposes stock/availability data to check against.

## Open questions / backend dependencies

- Order List remains fundamentally limited by the missing list endpoint — see "The contract still has no list endpoint" above. This is the single biggest open item for this module.
- The realtime event name/payload (`OrderStatusChanged`) is entirely a placeholder pending a published SignalR event catalog.
- If the Order contract ever adds payment/shipping/discount fields, `OrderItems`/a new `OrderPayment`/`OrderShipping` component would be the place to add them — the tab structure already accommodates new sections without redesign.
- `OrderCompletedIntegrationEvent` has no consumer yet — if Completed-order notifications are ever needed, that's a Notification Service change, not a frontend one.
- Route-level role guarding (`AuthGuard`'s `getRequiredRolesForPath`) trusts the roles the backend returns on `GetUserDetailResponse` — it's UI-layer defense, not a substitute for the backend's own `RequireAdmin`/`RequireAuthenticated` policy checks, which remain the actual enforcement.
