# Task 6: Redesign checkout into an explicit multi-step order flow with an editable order owner

**Status:** Not started. Backend now unblocks Phase A + address; only payment (Phase B's other half) remains blocked — see update below.

## Ask

Like other e-commerce platforms: the order owner (name/phone/address) must be enterable by the buyer, and is allowed to differ from the logged-in account (still approved). Redesign checkout into explicit steps:

1. **Cart** — show cart items, calculate and show the total.
2. **Order Owner** — name, address, phone, etc. Prefilled automatically from the user's own profile, but editable (maybe including shipping method later).
3. **Confirm** — maybe choose a payment method here too.
4. _(Maybe)_ a step where the user performs a transfer, then the order auto-completes.
5. **Complete** — Order ID, alert/message, etc.

## Current state

Single-page flow, not a wizard:

- `src/features/cart/components/CartPage.tsx:105` — "Proceed to checkout" is just `router.push('/checkout')`. Cart itself is real, backed by `useCartQuery`/mutations — step 1 above already exists in substance, just not as a formal "step."
- `src/features/checkout/components/CheckoutPage.tsx` is one component, conditionally rendered, not a step machine:
  - `:15-21` — pulls `cart` (`useCartQuery`) and `user` (`useCurrentUserQuery`), holds one local `placedOrder` state.
  - `:80-101` (`handlePlaceOrder`) builds the request as `customerName: toSessionUser(user).displayName`, `customerPhone: user.phoneNumber ?? ''` — **auto-derived from the session, with no form field and not editable**. This is the opposite of the ask: today it's forced-equal to the account, not "editable and allowed to differ."
  - `:103-119` — the form body is just `CheckoutSummary` (cart display) + one "place order" button. **No payment-method UI exists anywhere**, and `createOrder(request)` (`src/features/checkout/api/checkout.queries.ts:4,20`) only ever sends `customerName`/`customerPhone`/`items` — there's no field to carry a payment method even if a UI existed.
  - `:23-46` — if `placedOrder` is set, renders a success view **in the same component** (conditional render, not a separate route/step): title, `orderId`/`total`, "View Order"/"Continue Shopping" buttons. This already substantively covers step 5 ("Complete"), just not as its own route.

No address field is collected anywhere in this flow today.

## Backend readiness (from a paired backend session — see `SimpleShop/docs/tasks/2026-07-22/Task3_order-owner-checkout-flow-review.md`)

| Part of the ask                                           | Backend status                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| --------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Owner name/phone editable, allowed to differ from account | **Already supported.** `CreateOrderRequest` takes `customerName`/`customerPhone` as free text; the session only ever supplies `customerId` separately — the two are already decoupled server-side. This repo's UI is the only thing forcing them equal today.                                                                                                                                                                                  |
| Address                                                   | **Now supported, as of 2026-07-22.** `CreateOrderRequest`/`AdminCreateOrderRequest` both take a new required `shippingAddress` (free text, max 500 chars — same snapshot convention as `customerName`/`customerPhone`, no structured street/city/zip breakdown). `GetOrderResponse` returns it too. Backend migration (`AddOrderShippingAddress`) not yet applied to a live DB — confirm with backend before relying on a running environment. |
| Payment method + "transfer then auto-complete"            | **Still not supported — explicitly deferred by product decision**, not just unscoped. No payment/channel concept exists; `CompleteOrder` is `Admin`-only today, not callable by a buyer or a payment webhook. This remains the one real blocker for Phase B; raise a fresh backend task when it's picked up (see `SimpleShop/docs/tasks/2026-07-22/Task3_order-owner-checkout-flow-review.md`'s update).                                       |

## Suggested phased approach

**Phase A — frontend-only, unblocked today:**

- Convert `CheckoutPage` into an explicit multi-step flow (Cart review → Owner info → Confirm), reusing `useCartQuery`/`createOrder` as-is.
- Make the Owner-info step a real form: prefill `customerName`/`customerPhone` from `useCurrentUserQuery()` as defaults, but make both fields editable — remove the current hard-coded derivation in `handlePlaceOrder`.
- Add a `shippingAddress` field to the same Owner-info step now that the backend supports it (no prefill source — the account has no address on file — so this one starts blank).
- Turn the success state into its own step/route instead of a conditional render inside `CheckoutPage`, so it reads as a distinct "Order Complete" step (Order ID + message), matching the ask more literally.

**Phase B — payment only now, still blocked:**

- Payment-method selection + transfer-confirmation-triggers-auto-complete, once backend scopes a non-admin-only completion path (explicitly deferred, not yet a scoped task — raise fresh when picked up).

## Status

Not started. Phase A (now including the address field) has no backend blocker and can start immediately; payment remains blocked pending a fresh backend scoping task (see cross-ref).

**Cross-ref:** SimpleShop `docs/tasks/2026-07-22/Task3_order-owner-checkout-flow-review.md`.
