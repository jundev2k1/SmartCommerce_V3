# Task 6: Order Detail line items omit the Discount column

**Status:** Open.

## Source

Full-system business-requirements audit, 2026-07-27. Requirement: "Order Detail: Product, Variant, Quantity, Price, Discount, Line Total."

## Current state

`get-order.ts:5-12` correctly types `discount` and `lineTotal` on `GetOrderItemResponse`. But `OrderItems.tsx:25-56` only renders product/unitPrice/quantity/lineTotal columns — Discount is fetched from the API but never rendered. The component's own comment (lines 22-23) incorrectly claims "no variant/discount columns... since neither exists on this DTO" — that's wrong for Discount specifically (Variant genuinely doesn't exist on the backend yet, see Task 10 in this folder, but Discount does).

## Suggested acceptance criteria

- Add a Discount column/value to the order item rows in `OrderItems.tsx`.
- Correct the stale comment (see Task 12 in this folder).

**Cross-ref:** none — pure frontend fix, data already available.
