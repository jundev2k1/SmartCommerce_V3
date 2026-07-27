# Task 7: Order list table shows raw customerId instead of customerName

**Status:** Open.

## Source

Full-system business-requirements audit, 2026-07-27.

## Current state

`OrdersListPage.tsx:114-132` renders the customer column from `customerId` (a GUID), even though `customerName` is available on the same response shape and is already displayed correctly on the order detail page's customer tab (`OrderCustomer.tsx`, per the 2026-07-22 Task 5 fix).

## Suggested acceptance criteria

- Order list shows the customer's name, not their raw ID.

**Cross-ref:** none — pure frontend fix, data already available. Related to Task 1 in this folder (once the list switches to the real search endpoint, make sure the customer-name column comes along).
