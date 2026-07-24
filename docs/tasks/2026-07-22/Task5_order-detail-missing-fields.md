# Task 5: Order Detail only shows a raw customer ID — show more detail

**Status:** Done. Updated types and UI, verified via typecheck + eslint.

## Ask

Order Detail currently only shows the customer's raw ID. Show more.

## Current state (this repo)

`GetOrderResponse` type, `src/services/order/get-order.ts:13-21`:

```ts
export interface GetOrderResponse {
  id: string;
  customerId: string;
  status: OrderStatus;
  totalAmount: number;
  items: GetOrderItemResponse[] | null;
  createdAt: string;
  updatedAt: string;
}
```

`OrderCustomer.tsx` (`src/shared/commerce/OrderCustomer.tsx:6-31`) takes only `{ customerId: string }` and renders just the raw id with an explanatory note (:10-16 docstring): _"`GetOrderResponse` only has a raw `customerId`... There's no cross-service lookup available either... So this shows the id plainly."_ Called from `OrderDetailPage.tsx:110`. Every other Order Detail sub-component (`OrderSummaryCard`, `OrderMetadata`, `OrderItems`, `OrderTimeline`) only reads fields that exist on this same narrow type.

## This assumption is now out of date — the backend already returns more

A paired backend investigation (see `SimpleShop/docs/tasks/2026-07-22/Task3_order-owner-checkout-flow-review.md`) confirms the **actual current** backend `GetOrderResponse` (`Order.Application/Features/Orders/Queries/GetOrder/GetOrderQuery.cs:13-22`) is:

```csharp
public sealed record GetOrderResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    string ShippingAddress,
    OrderStatus Status,
    decimal TotalAmount,
    IReadOnlyCollection<GetOrderItemResponse> Items,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt);
```

with items carrying `Discount`/`LineTotal` in addition to what this repo's `GetOrderItemResponse` type already has. `CustomerName`/`CustomerPhone` are genuinely persisted on the `Order` aggregate (not transient) — confirmed via a migration (`AddOrderCustomerSnapshotAndDiscount`) that landed on the backend **today**, 2026-07-22.

**`ShippingAddress` was added later the same day** (migration `AddOrderShippingAddress`, not yet applied to a live DB) as part of the checkout-redesign backend work — see Task 6's own update for the full picture of what's now unblocked.

**This repo's `docs/backend/order/README.md` mirror doc is now stale too** — it currently states these DTOs are "unchanged," written before that migration landed.

## What to do

1. Update `src/services/order/get-order.ts`'s `GetOrderResponse`/`GetOrderItemResponse` types to add `customerName`, `customerPhone`, `shippingAddress`, `cancellationReason`, and per-item `discount`/`lineTotal` — matching the backend's actual current response.
2. Update `OrderCustomer.tsx` to accept and render `customerName`/`customerPhone` instead of (or alongside) the raw id — this removes the need for the "no data available" framing entirely for these two fields.
3. Update `docs/backend/order/README.md` to reflect the new response shape (per that doc's own "don't let docs drift" rule).
4. `CancellationReason`, `ShippingAddress`, and item-level `Discount`/`LineTotal` are separately available now too — decide whether/where to surface them (e.g. `OrderTimeline` for cancellation reason, `OrderItems` for discount/line total, an address block near `OrderCustomer`) as part of the same pass, since the data is already there.

## Implementation

1. **Types:** Updated `GetOrderResponse` in `src/services/order/get-order.ts` to include `customerName`, `customerPhone` (both required), `shippingAddress?`, `cancellationReason?` (optional). Updated `GetOrderItemResponse` to include `discount?` (already had `lineTotal`).

2. **Component:** Updated `OrderCustomer.tsx` to accept and render the new `customerName`/`customerPhone` fields in a 3-field layout (name, phone, id). Removed the "id only" explanatory note since we now have full customer data.

3. **Caller:** Updated `OrderDetailPage.tsx` to pass the new props when instantiating `OrderCustomer`.

4. **i18n:** Updated `commerce.json` to add "name", "phone", "id" labels and remove the obsolete "idOnlyNote" copy.

5. **Docs:** Updated `docs/backend/order/README.md` to document the new shape of `GetOrderResponse` and `GetOrderItemResponse`.

**Not done (out of scope for this pass):** Surfacing `shippingAddress`, `cancellationReason`, per-item `discount` in the detail UI — those can be added separately once decisions are made on where they belong (e.g. shipping address block, timeline/cancellation notes, item line totals).

Verified: `tsc --noEmit` and `eslint` both clean.

**Cross-ref:** SimpleShop `docs/tasks/2026-07-22/Task3_order-owner-checkout-flow-review.md`.
