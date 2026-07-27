# Task 3: Order edit UI only supports owner info — no item edit, no delete

**Status:** Open.

## Source

Full-system business-requirements audit, 2026-07-27.

## Current state

The only wired order-edit mutation is `useUpdateOrderOwnerInfoMutation` (`services/order/update-order-owner-info.ts:22`), covering phone/address only. There is no frontend client for `PUT /orders/{id}` (line-item update, Pending-only per backend rule — `UpdateOrder.cs:36`/`UpdateOrderHandler.cs:20`) or `DELETE /orders/{id}` (admin hard-delete — `DeleteOrder.cs:28`), even though both exist and work on the backend.

## Why this matters

"CRUD Order" is an explicit requirement; Update (of order contents, not just owner info) and Delete are both present on the backend but entirely unreachable from the UI.

## Suggested acceptance criteria

- New `src/services/order/update-order.ts` and `delete-order.ts` client wrappers.
- Order Detail page gains an item-edit flow (Pending orders only) and a delete action (admin-only, with confirmation).

**Cross-ref:** none — pure frontend catch-up, backend already supports this.
