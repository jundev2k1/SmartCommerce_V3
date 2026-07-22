# Backend: Order Service

**Purpose:** What the Order Service exposes and how `src/services/order` maps to it.

**Scope:** This service's contract only.

**Related documents:** [backend/README.md](../README.md), [backend/feature-mapping.md](../feature-mapping.md), [backend/coverage-report.md](../coverage-report.md), `src/services/order/`

**When to read:** Building Checkout/Orders (Phases 7–8).

**When to ignore:** Any work not touching orders.

---

## Purpose

Cart, order creation, retrieval, and cancellation. Order items reference products/variations by id and are priced/validated against "the local product catalog" (a read replica or cache the Order service owns, per its own description — not a live call to the Product service on every request). Cart lines are priced live from the catalog on every read (see "Cart" below) — not a snapshot the way order items are.

## Base path

`/api/order`.

## Auth requirements

Not explicitly documented per-endpoint in this contract; assume standard authenticated-session requirements once Auth (Phase 2) lands. The user for Cart and `POST /orders` is always resolved server-side from the auth session — never from a request body/query param. Exceptions, confirmed from backend source: `CompleteOrder` and `POST /orders/admin` require the `Admin`/`Root` role (`RequireAdmin` policy) — every other endpoint here only requires an authenticated session.

## Endpoints

| Operation            | Method & Path                      | Client function                                |
| -------------------- | ---------------------------------- | ---------------------------------------------- |
| Get cart             | `GET /cart`                        | `getCart()`                                    |
| Add cart item        | `POST /cart/items`                 | `addCartItem(request)`                         |
| Update cart item qty | `PATCH /cart/items/{variationId}`  | `updateCartItemQuantity(variationId, request)` |
| Remove cart item     | `DELETE /cart/items/{variationId}` | `removeCartItem(variationId)`                  |
| Clear cart           | `DELETE /cart`                     | `clearCart()`                                  |
| Create order         | `POST /orders`                     | `createOrder(request)`                         |
| Create order (admin) | `POST /orders/admin`               | `adminCreateOrder(request)`                    |
| Get order            | `GET /orders/{orderId}`            | `getOrder(orderId)`                            |
| Cancel order         | `POST /orders/{orderId}/cancel`    | `cancelOrder(orderId)`                         |
| Complete order       | `POST /orders/{orderId}/complete`  | `completeOrder(orderId)`                       |

`CompleteOrder` is admin-only (`RequireAdmin`) — moves a `Confirmed` order to `Completed`. Confirmed by backend source (`Order.API/Endpoints/CompleteOrder.cs`, `Order.Domain/Entities/Order.cs`'s `Complete()`), not Swagger prose — added alongside the frontend's Admin "Approve Orders" view since no endpoint previously existed for it.

`POST /orders/admin` is also admin-only (same `RequireAdmin` policy) — creates an order on behalf of a specified customer, since there's no cart to source one from. No cart-diff check applies to this path. It has no frontend UI yet — see [modules/client-mock.md](../../modules/client-mock.md) and [backlog.md](../../backlog.md).

## Cart

Redis-backed, server-side, one cart per authenticated user. Every read (`GET /cart`, and the response of every mutation below) re-resolves name/price/availability from the product catalog — a line whose variation was deleted or made unavailable since it was added is silently dropped, not returned and not errored. This means `totalAmount`/prices can change between two `GET /cart` calls with no explicit mutation in between, if a product's price changed server-side.

`POST /cart/items` adds to an existing line's quantity if the variation is already present. `PATCH /cart/items/{variationId}` rejects `quantity <= 0` with 400 — it never deletes the line; use the `DELETE` endpoint for that (which itself is a no-op, still 200, if the variation wasn't in the cart).

## Request/response DTOs

```ts
// Cart
CartItemResponse { productId, variationId, productName, unitPrice, quantity }
CartResponse { items: CartItemResponse[], totalAmount } // clear cart returns no body

// Orders — no `customerId` on the client-facing request; the caller's own
// session is always the identity placing the order.
CreateOrderRequest { customerName, customerPhone, items: CreateOrderItemRequestDto[] }
CreateOrderItemRequestDto { productId, variationId, quantity, discount? }
CreateOrderResponse { orderId, totalAmount, status } // identical shape for both POST /orders and POST /orders/admin

// Admin-only — customerId required, since there's no cart to source it from
AdminCreateOrderRequest { customerId, customerName, customerPhone, items: CreateOrderItemRequestDto[] }
```

`GetOrderResponse` (includes `items: GetOrderItemResponse[]`), `CancelOrderResponse { orderId, status }`, `CompleteOrderResponse { orderId, status }` are unchanged. See `src/services/order/`.

## Pagination style

N/A — no list endpoint in this contract (order history/listing isn't exposed yet — see [coverage-report.md](../coverage-report.md) "Missing endpoints").

## Error responses

Prose-only, no formal error schema:

- `POST /cart/items` → 404 (variation doesn't exist in the catalog), 400 (exists but not currently orderable).
- `PATCH /cart/items/{variationId}` → 404 (variation not currently in the cart), 400 (`quantity <= 0`).
- `POST /orders` → 400 (invalid/validation), 404 (product not found in local catalog), **409** (`items` doesn't match the caller's current server-side cart exactly — the client must `GET /cart` and resubmit; see "Cart" above for how a cart can drift, e.g. two tabs or a silently-dropped unavailable line).
- `POST /orders/admin` → 400, 404 — same item validation as `POST /orders`, but no 409/cart-diff check.
- `GetOrder` → 404, 400 (bad id format).
- `CancelOrder` → 404, 400 (already cancelled/completed).

Item validation now checks stock availability _before_ the catalog lookup (previously the reverse) — the only visible effect is that an insufficient-stock error message references the raw `variationId` rather than the resolved product name, since the catalog lookup hasn't happened yet at that point. No frontend change needed: [services/error-handling.md](../../services/error-handling.md)'s toast rendering treats `message` as opaque text.

## Known limitations / TODOs

- [ ] **No list/search endpoint for orders.** "Order History" (a named module in [modules/overview.md](../../modules/overview.md)) has no backing `GET /orders` list endpoint in this contract — only get-by-id. Phase 8 will need to either wait for that endpoint or clarify with the backend how an order list is meant to be sourced.
- [x] `OrderStatus` (1–4) mapping is now confirmed from backend source (`Order.Domain/Enums/OrderStatus.cs`): `Pending=1, Confirmed=2, Cancelled=3, Completed=4`. Rendered by `types/order-status.ts`/`OrderStatusBadge`.
- [x] Cart previously didn't exist on any backend service — now a real Redis-backed contract (see "Cart" above). `CreateOrderItemRequestDto` previously had no variation-level field — it now carries `variationId`.
