# Task 4: No conflict-resolution UI for concurrent order edits

**Status:** Blocked — depends on SimpleShop `docs/tasks/2026-07-27/Task2_order-concurrency-token-not-in-contract.md`.

## Source

Full-system business-requirements audit, 2026-07-27. Requirement: verify UI behavior for concurrent order edits.

## Current state

The only wired edit mutation (`useUpdateOrderOwnerInfoMutation`, `EditOrderOwnerInfoForm.tsx:29-38`) sends no version/rowversion token today (because the backend doesn't have one to send — see the blocking backend task) and its error handling only special-cases 400/403. A 409 conflict response falls through to a generic `toast.error` with no reload/retry/diff flow. (`checkout.json:18`'s existing 409 handling is for cart-mismatch on order _creation_, unrelated to edit conflicts.)

## Why this matters

This is the UI half of "two users editing the same order simultaneously" — without it, a genuine conflict is indistinguishable from any other generic error, and the user has no path to recover except guessing to reload and retry manually.

## Suggested acceptance criteria (once unblocked)

- Order edit forms send back whatever version token `GetOrder` returns.
- A 409 response is caught specifically and shown as a clear "this order changed since you loaded it" message with a one-click reload-and-retry action.

**Cross-ref:** SimpleShop `docs/tasks/2026-07-27/Task2_order-concurrency-token-not-in-contract.md`.
