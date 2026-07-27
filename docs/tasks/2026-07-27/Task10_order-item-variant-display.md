# Task 10: Render Variant once it exists on the backend OrderItem DTO

**Status:** Blocked — depends on SimpleShop `docs/tasks/2026-07-27/Task5_order-item-missing-variant-field.md`.

## Source

Full-system business-requirements audit, 2026-07-27.

## Current state

The backend has no distinct variant field on order items today — `ProductId` doubles as the variation id with only `ProductName` for display (see the blocking backend task).

## Suggested acceptance criteria (once unblocked)

- Render variant (e.g. size/color/SKU) alongside product name in `OrderItems.tsx`, distinct from the product name.

**Cross-ref:** SimpleShop `docs/tasks/2026-07-27/Task5_order-item-missing-variant-field.md`.
