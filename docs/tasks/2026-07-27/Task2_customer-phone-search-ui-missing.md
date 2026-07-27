# Task 2: Customer search by phone (prefix + suffix) has no UI at all

**Status:** Open.

## Source

Full-system business-requirements audit, 2026-07-27. Requirement: "Search by phone number — prefix, suffix."

## Current state

The backend (`User` service) has a fully indexed prefix (`StartsWith` against a normalized `PhoneSearch` column) and suffix (`EndsWith`, implemented as an indexed `StartsWith` against a reversed `PhoneReverse` column) phone search — both genuinely indexed, per `PhoneSearchStrategy.cs` and `UserProfileConfig.cs` on the backend.

But `src/services/user/search-users.ts` / `users.queries.ts` never send a `phone` filter — only `keyword` and `role`. There is no phone input field anywhere in `src/features/users/`.

## Why this matters

"Search by phone number (prefix/suffix)" is an explicit, named business requirement. Backend readiness is 100%; frontend exposure is 0% — this is purely a missing UI, not a missing capability.

## Suggested acceptance criteria

- Add a phone search input to the Users search form.
- Wire it to send `filters: [{ field: "phone", operator: "sw" }]` for prefix and `"ew"` for suffix (or however the UI chooses to expose the two modes — e.g. a single input with a toggle, or two inputs).
- Searching "098" (prefix) returns customers whose phone starts with 098; searching "8888" (suffix) returns customers whose phone ends with 8888.
- Pagination works with the phone filter active.

**Cross-ref:** none — no backend change needed, this is frontend-only.
