# Modules Overview

**Purpose:** The full list of business modules this admin dashboard will eventually have, and which nav entries are reserved-but-disabled today.

**Scope:** Module inventory and navigation reservation only. Per-module implementation detail is written into `docs/modules/<name>.md` when that module's phase actually starts (see [_template.md](./_template.md)) — this file does not contain implementation detail itself.

**Related documents:** [roadmap.md](../roadmap.md), [_template.md](./_template.md), [frontend/routing.md](../frontend/routing.md)

**When to read:** Building the nav config (Phase 3), or checking whether a module has a dedicated doc yet.

**When to ignore:** Working inside an already-documented module — read that module's own doc instead.

---

## Module list

| Module                                                     | Status                                                                                                                                          | Roadmap phase                                                                      | Doc                                                                 |
| ---------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------- | ------------------------------------------------------------------- |
| Authentication                                             | Implemented                                                                                                                                     | Phase 2                                                                            | [authentication.md](./authentication.md)                            |
| User Management                                            | Implemented (partial — no list/delete, see doc)                                                                                                 | Phase 2 _(moved up from Phase 4 — built alongside Auth as the first admin module)_ | [user-management.md](./user-management.md)                          |
| Product Management                                         | Implemented (includes Variants — see doc)                                                                                                       | Phase 5                                                                            | [product-management.md](./product-management.md)                    |
| Product Categories                                         | Implemented                                                                                                                                     | Phase 5                                                                            | [product-categories.md](./product-categories.md)                    |
| Product Tags                                               | Implemented (no color/usage-count — unsupported, see doc)                                                                                       | Phase 5                                                                            | [product-tags.md](./product-tags.md)                                |
| Product Search                                             | Implemented                                                                                                                                     | Phase 6                                                                            | [product-search.md](./product-search.md)                            |
| Shop (customer-facing browse)                              | Implemented (temporary Client Mock — see doc)                                                                                                   | Phase 4 (Client Mock)                                                              | [client-mock.md](./client-mock.md)                                  |
| Shopping Cart                                              | Implemented (client-only, no cart API — see doc)                                                                                                | Phase 7 → merged into Client Mock                                                  | [client-mock.md](./client-mock.md)                                  |
| Checkout                                                   | Implemented                                                                                                                                     | Phase 7 → merged into Client Mock                                                  | [client-mock.md](./client-mock.md)                                  |
| Order History / Orders (admin management) / Order Approval | Implemented (no list endpoint — local-order-id tracking; realtime via placeholder SignalR event; Orders/Order Approval are Admin-only, see doc) | Phase 8 → upgraded in Phase 6 "Sales"                                              | [order-management.md](./order-management.md)                        |
| Inventory (Stock)                                          | Implemented (no list endpoint — local-tracking, see doc)                                                                                        | Phase 9 (Phase 5 "Inventory")                                                      | [inventory-management.md](./inventory-management.md)                |
| Warehouses                                                 | Implemented (no Update/Delete/List endpoints — see doc)                                                                                         | Phase 9 (Phase 5 "Inventory")                                                      | [inventory-management.md](./inventory-management.md)                |
| Stock Transactions                                         | Implemented (aggregated client-side — see doc)                                                                                                  | Phase 9 (Phase 5 "Inventory")                                                      | [inventory-management.md](./inventory-management.md)                |
| Notifications (in-app + realtime)                          | Implemented (no cursor/unread-count endpoints — see doc)                                                                                        | Phase 10 (Phase 7 "Notification Center")                                           | [notification-center.md](./notification-center.md)                  |
| Audit                                                      | Planned                                                                                                                                         | Phase 11                                                                           | _(created at Phase 11)_                                             |
| SignalR Realtime                                           | Cross-cutting (not a standalone nav entry)                                                                                                      | Phase 10                                                                           | See [realtime/signalr-strategy.md](../realtime/signalr-strategy.md) |
| Notification Campaigns                                     | **Reserved, not implemented**                                                                                                                   | Not scheduled                                                                      | —                                                                   |
| Notification Rules                                         | **Reserved, not implemented**                                                                                                                   | Not scheduled                                                                      | —                                                                   |
| Notification Groups                                        | **Reserved, not implemented**                                                                                                                   | Not scheduled                                                                      | —                                                                   |
| Notification Channels                                      | **Reserved, not implemented**                                                                                                                   | Not scheduled                                                                      | —                                                                   |

## Bounded context grouping

Sidebar navigation groups modules by bounded context, not flat/alphabetical, so the nav mirrors the domain model as the module count grows:

| Bounded Context  | Modules                                                                                                                                                                   |
| ---------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **IAM**          | Authentication, User Management                                                                                                                                           |
| **Catalog**      | Product Management (includes Variants — no independent nav entry, see [product-management.md](./product-management.md)), Product Categories, Product Tags, Product Search |
| **Sales**        | Shop, Shopping Cart, Checkout, Orders, Order Approval, Order History                                                                                                      |
| **Inventory**    | Inventory, Warehouses, Stock Transactions                                                                                                                                 |
| **Notification** | Notifications (in-app + realtime), Notification Campaigns*, Notification Rules*, Notification Groups*, Notification Channels*                                             |
| **System**       | Audit                                                                                                                                                                     |

\* Reserved, not implemented — see below.

This grouping is implemented in `shared/config/nav.ts` (Phase 1) and should stay in sync with this table as modules are added or regrouped.

## Navigation reservation rule

The four Notification-admin entries (Campaign, Rule, Group, Channel) must appear in `shared/config/nav.ts` from Phase 3 onward, marked `implemented: false`, and render disabled/"coming soon" in the shell — see [roadmap.md](../roadmap.md) Phase 3 and Phase 10. This reserves their place in the information architecture so adding them later doesn't reshuffle the nav for users who've learned its layout, without building any of their functionality now.

## Role-based navigation (Admin vs. User)

`shared/config/nav.ts`'s `NavGroup`/`NavItem` both accept an optional `roles?: string[]` (item-level overrides group-level; omitting both means "visible to any authenticated role"). IAM, Catalog, Inventory, and System are wholesale `roles: [AppRole.Admin]` at the group level; Sales and Notification mix restricted and unrestricted items within the same group (e.g. Shop/Cart/Checkout/Order History stay open to `User`, while the admin Orders table, Order Approval, and the four reserved Notification-admin placeholders are `Admin`-only). `AppRole` (`Root`/`Admin`/`User`, mirroring the backend's `BuildingBlock.SharedKernel.Constants.AppRole`) lives in `src/services/user/types/app-role.ts`.

Two mechanisms consume this, and are kept in sync by reading the same `navGroups` data rather than duplicating a route table:

- **`shared/layout/Sidebar.tsx`** filters `navGroups`/their `items` against the session's `user.roles` (`hasRequiredRole`, also exported from `nav.ts`) before rendering — a restricted entry simply isn't in the DOM, not just disabled.
- **`features/auth/components/AuthGuard.tsx`** calls `getRequiredRolesForPath(pathname)` (also in `nav.ts`) on every route change inside `(admin)` and redirects to `/403` if the session's roles don't satisfy it — so a `User`-role account typing an admin URL directly (e.g. `/products`) is blocked, not just kept from seeing the nav entry. `getRequiredRolesForPath` exact-matches a pathname against a nav item's `href` first, then falls back to prefix-matching for nested routes not themselves listed in nav.ts (e.g. `/products/[productId]` inherits `/products`'s restriction) — except `/orders`, deliberately excluded from prefix matching since `/orders/[orderId]` is a detail page any authenticated role can reach even though the `/orders` admin list itself is Admin-only (see [order-management.md](./order-management.md)).
- `Root` satisfies every role check on both the frontend (`hasRequiredRole`) and the backend (`ClaimsPrincipalExtensions.HasRole`) — a superuser account is never hidden from or blocked by role-gated nav/routes.

This is UI-layer defense only — it hides nav entries and blocks route rendering, it does not replace the backend's own per-endpoint authorization policies (`RequireAdmin`/`RequireAuthenticated`), which remain the actual enforcement if someone calls the API directly.

## Creating a module doc

When a module's phase starts, copy [_template.md](./_template.md) to `docs/modules/<name>.md`, fill it in, and update this table's "Doc" column with a link. Don't pre-create these docs speculatively — they tend to go stale before the module is real.
