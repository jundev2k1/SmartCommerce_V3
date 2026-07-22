# Client Mock (Shop, Cart, Checkout, Orders)

**Purpose:** A temporary, realistic customer-facing flow — Shop (product search + detail), Cart, Checkout, and My Orders — built inside the admin dashboard to validate the real Product and Order APIs end to end. **Not** the final customer website; it may later be extracted into a dedicated client application.

**Scope:** `features/shop`, `features/cart`, `features/checkout`, `features/orders`, and the `shared/commerce` component layer they share.

**Related documents:** [modules/overview.md](./overview.md), [modules/product-management.md](./product-management.md), [modules/product-search.md](./product-search.md), [backend/product/README.md](../backend/product/README.md), [backend/order/README.md](../backend/order/README.md), [decisions/0015-shared-commerce-component-layer.md](../decisions/0015-shared-commerce-component-layer.md), [state/zustand-strategy.md](../state/zustand-strategy.md).

**When to read:** Touching Shop/Cart/Checkout/Orders, or the `shared/commerce` components.

**When to ignore:** Admin Catalog work (Products/Categories/Tags) — see [product-management.md](./product-management.md) instead.

---

## Feature folder(s)

`src/features/shop/`, `src/features/cart/`, `src/features/checkout/`, `src/features/orders/`. Shared presentation lives in `src/shared/commerce/`. Cart is server state, fetched/mutated via `src/features/cart/api/cart.queries.ts` (TanStack Query) — no client-only cart store anymore. `src/shared/stores/local-orders.store.ts` remains the one client-only store in this flow.

## Key flows

Shop (search/filter → product detail → select variant + quantity → add to cart) → Cart (edit quantities/remove/clear → proceed to checkout) → Checkout (review → place order → success view) → My Orders (list orders placed from this browser → order detail → cancel).

Checkout builds its `CreateOrderRequest.items` directly from the live `useCartQuery()` data at submit time — no separate local snapshot. The server rejects the request with 409 if `items` doesn't match the caller's current server-side cart exactly (e.g. drifted via another tab, or a line silently dropped because its variation became unavailable); on that path the cart query is invalidated and a specific "cart changed, review it" toast is shown instead of the generic error toast. On success, the server clears the cart itself — the frontend only invalidates its cached copy, it doesn't separately reset any client-side cart state.

## Routes

`/shop`, `/shop/[productId]`, `/cart`, `/checkout`, `/orders`, `/orders/[orderId]` — all under `(admin)`, reusing the existing admin shell per the brief ("do not create a completely different application shell").

## Why this is a separate bounded context from Catalog

`features/products`/`features/product-search` are admin tools (CRUD, or an admin-facing browse page). `features/shop` is the customer-facing equivalent — it reuses `features/products`' `useSearchProductsQuery`/`useProductQuery`/`ProductFilters` via its public barrel (no duplicate query logic), but renders through `shared/commerce`'s `ProductCard`/`ProductGrid` with an "Add to cart" action and links into `/shop/[id]` instead of the admin detail page. `features/product-search`'s admin page was refactored to consume the same `ProductCard`/`ProductGrid` (linking to `/products/[id]` instead) — the two pages now share the card/grid presentation, differing only in the action slot and link target.

## Real contract gaps and how each was handled (not invented)

- **The Order service has no list/search endpoint** (see `backend/order/README.md` "Known limitations"). "My Orders" is backed by `shared/stores/local-orders.store.ts`, which records the `orderId` returned by every successful checkout on this browser; the list page then calls the real `GetOrder` for each id via `useLocalOrdersQuery` (`features/orders/api/orders.queries.ts`). Every field shown is real backend data — the workaround only supplies _which ids_ to fetch. Orders placed from another browser/device, or before this feature existed, will not appear. This was confirmed with the project owner as the preferred approach over a bare placeholder, given Checkout already has the id in hand right after creating the order.
- **`OrderStatus` (1–4) mapping is now confirmed from backend source** (`Order.Domain/Enums/OrderStatus.cs`: `Pending=1, Confirmed=2, Cancelled=3, Completed=4`) — `OrderStatusBadge` renders real Pending/Confirmed/Cancelled/Completed labels. `OrderTimeline` is still a literal placeholder for history — `GetOrderResponse` has no status-history array, so it shows only the current status plus a note that history isn't exposed; that part of the gap is unrelated to the status-name mapping and remains open.
- **Cancel eligibility is still not client-side guessed**, even though the status names are now known — the terminal-state check (already cancelled/completed) is left to the server's real 400, which surfaces as a normal error toast, the same "let the server enforce it" pattern used for the last-variation-can't-be-deleted rule in Product Management.
- **No availability/stock check.** Inventory (Phase 9) doesn't exist yet, so neither Product Detail nor Checkout validates stock — "Availability (if API provides it)" from the brief is explicitly N/A for this phase.

## Reusable components (`shared/commerce/`)

`ProductPrice`, `ProductGrid`, `ProductCard`, `VariantSelector`, `QuantitySelector`, `CartSummary`, `CheckoutSummary`, `OrderStatusBadge`, `OrderTimeline` — see [decisions/0015-shared-commerce-component-layer.md](../decisions/0015-shared-commerce-component-layer.md) for why this is a distinct layer from `shared/entity`/`shared/ui`.

`VariantSelector` hides variations whose free-text `status` is `"Inactive"`/`"Discontinued"` — a presentation choice for a customer-facing picker (a storefront shouldn't offer a discontinued SKU), not an invented backend rule; the status field itself is real data.

## Dependencies on other modules

Depends on **Product Management** (`useSearchProductsQuery`, `useProductQuery`), **Product Categories**/**Product Tags** (category/tag name lookups on the Shop product detail page, same pattern as `ProductCategoriesTab`/`ProductTagsTab`), and **Authentication** (`useCurrentUserQuery` supplies `customerName`/`customerPhone` Checkout sends to `CreateOrder` — there's no `customerId` on this request at all anymore; the caller's identity is resolved server-side from the auth session).

## Prices have no currency

Neither the Product nor Order contract exposes a currency field. `ProductPrice` renders a plain 2-decimal number, matching the `.toFixed(2)` convention already used in `features/products` — inventing a `$`/`USD` symbol wasn't done because there's nothing backing it in either contract.

## Open questions / backend dependencies

- If the Order service ever adds a list/search endpoint, `useLocalOrdersQuery` should be replaced with a real paginated query — the local-order-id tracking becomes unnecessary at that point (`shared/stores/local-orders.store.ts` can be removed).
- `POST /orders/admin` (create an order on behalf of a specified customer) has no frontend UI yet — logged as available-but-not-built in [backlog.md](../backlog.md) §5, not silently ignored.
