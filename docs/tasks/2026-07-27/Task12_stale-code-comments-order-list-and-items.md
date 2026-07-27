# Task 12: Stale code comments claiming missing endpoints

**Status:** Partially done — `OrdersListPage.tsx:23`'s stale comment is gone as a side effect of Task 1's rewrite (the `useLocalOrdersQuery` call it was attached to no longer exists on that page). `OrderItems.tsx:22-23` is still open, blocked on Task 6 landing as originally scoped.

## Source

Full-system business-requirements audit, 2026-07-27.

## Current state

`OrdersListPage.tsx:23` and `OrderItems.tsx:22-23` contain comments asserting "no order list endpoint exists" and "no variant/discount columns since neither exists on this DTO" respectively. Both are now stale — the backend does have a `SearchOrders` endpoint (Task 1 in this folder), and Discount does exist on the DTO already (Task 6 in this folder); only Variant is genuinely absent.

## Suggested acceptance criteria

- Update or remove these comments once Tasks 1 and 6 land, so future readers aren't misled.

**Cross-ref:** Tasks 1 and 6 in this folder.
