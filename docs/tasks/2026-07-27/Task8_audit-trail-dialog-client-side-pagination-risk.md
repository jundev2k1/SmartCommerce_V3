# Task 8: Audit trail dialog's client-side pagination workaround will silently miss data at scale

**Status:** Blocked (fully) — depends on SimpleShop `docs/tasks/2026-07-27/Task6_audit-log-missing-entity-filter.md`. The `console.log` cleanup below is unblocked and can be done now.

## Source

Full-system business-requirements audit, 2026-07-27.

## Current state

`AuditTrailDialog.tsx:34-57` pages up to 5×50=250 records per service and filters by `entityId` in the browser, since the backend has no server-side entity filter. Beyond 250 events for a given service, older entries for any entity become invisible with no indication to the user that the view is incomplete. There's also a stray `console.log(entries)` debug statement left in at `AuditTrailDialog.tsx:78`.

## Suggested acceptance criteria

- Remove the stray `console.log(entries)` now (unblocked, independent of the backend task).
- Once the backend's server-side `rootEntityType`/`rootEntityId` filter exists, switch `AuditTrailDialog.tsx` to call it directly instead of paging-and-filtering client-side.

**Cross-ref:** SimpleShop `docs/tasks/2026-07-27/Task6_audit-log-missing-entity-filter.md`.
