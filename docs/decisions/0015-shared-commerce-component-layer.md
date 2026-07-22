# 0015 — `shared/commerce/`: Reusable Customer-Facing Commerce Components

**Status:** Accepted

**Date:** 2026-07-20 (Phase 4 — Client Mock)

## Context

Phase 4 adds a temporary "Client Mock" module (Shop, Cart, Checkout, Orders) to exercise the Product and Order contracts end to end from a customer's perspective, ahead of a real customer-facing application existing. Several presentation patterns repeat across its pages: a product card usable from both the customer shop and the existing admin Product Search page, a price renderer, a variant picker, a quantity stepper, cart/checkout totals boxes, and an order status indicator — none of which fit `shared/entity` (admin-CRUD scaffolding) or `shared/ui` (third-party wrappers).

## Decision

Add `shared/commerce/`: `ProductPrice`, `ProductGrid`, `ProductCard`, `VariantSelector`, `QuantitySelector`, `CartSummary`, `CheckoutSummary`, `OrderStatusBadge`, `OrderTimeline`. One barrel, consumed the same way `shared/ui`/`shared/entity` are.

`ProductCard` is used by both `features/shop` (new) and `features/product-search` (Phase 3's admin browse page, refactored to consume it instead of its inline card markup) — the first real cross-module reuse of a commerce-specific component, not just a hypothetical one.

## Rationale

- Distinct from `shared/entity`: these components have no notion of admin CRUD (no delete-confirm, no audit trail, no bulk selection) — they're customer-facing commerce primitives.
- Distinct from `shared/ui`: these compose multiple `shared/ui` primitives with commerce-specific opinions (a price format, a cart line-item shape), not a single third-party wrapper.
- `OrderStatusBadge` and `OrderTimeline` exist specifically to contain a real contract gap in one place: `OrderStatus` (see `docs/backend/order/README.md`) is an opaque 1–4 enum with no published name mapping. Rather than invent a Pending/Confirmed/Processing/Shipping/Completed/Cancelled mapping the brief asked for, both components render only what the backend actually publishes (the raw numeric value, and "no timeline data available"). Centralizing this means the moment the backend publishes the mapping, only these two files change.

## Alternatives considered

- **Guess the `OrderStatus` mapping** (e.g. assume 1=Pending...4=Cancelled) to satisfy the brief's requested status labels literally: rejected — this is exactly the kind of invented backend behavior the project's standing rule forbids. `CancelOrder`'s prose only confirms that "cancelled" and "completed" exist among the four values, not which numbers they are.
- **Duplicate `ProductCard` per module** (one for admin Product Search, one for Shop): rejected for the same reason `shared/entity` was extracted in Phase 3 — the repetition is real and immediate, not speculative.
- **Fold these into `shared/entity`**: rejected — `shared/entity` is scoped to admin-CRUD screen scaffolding per ADR 0014; blurring it with customer-facing commerce components would make both harder to reason about.

## Consequences

- Any future real customer-facing application (or a genuine Sales/Catalog admin screen needing a price/status renderer) reaches for `shared/commerce` first.
- If/when the Order service publishes a real `OrderStatus` name mapping, `OrderStatusBadge`/`OrderTimeline` are the only files that need updating — no feature code references the raw numeric status directly.
- `shared/commerce` intentionally has no cart-mutation logic of its own — `CartSummary`/`CheckoutSummary` are presentational, driven by props; the actual cart state lives in `shared/stores/cart.store.ts` per `docs/state/zustand-strategy.md`.

## Update (Phase 6 — Sales / Order Management)

Extended rather than superseded: `OrderTimeline` gained an `events` prop (realtime-observed status changes, since `GetOrderResponse` still has no history array) alongside its existing `currentStatus`-only mode. Five new components — `OrderSummaryCard`, `OrderItems`, `OrderCustomer`, `OrderActions`, `OrderMetadata` — were added, extracted from what was previously inline markup in `OrderDetailPage`, once the admin Order Management page needed the same sections a printable/alternate order view eventually would. All five follow the same prop-driven, no-internal-fetching rule as the rest of this layer. See [modules/order-management.md](../modules/order-management.md).

## Related documents

[architecture/overview.md](../architecture/overview.md), [decisions/0014-shared-entity-component-layer.md](./0014-shared-entity-component-layer.md), [modules/client-mock.md](../modules/client-mock.md), [modules/order-management.md](../modules/order-management.md), [backend/order/README.md](../backend/order/README.md)
