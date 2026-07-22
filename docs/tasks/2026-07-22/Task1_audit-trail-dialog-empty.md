# Task 1: Audit Trail dialog shows empty despite a successful API response

**Status:** Done. Fix applied — bounded multi-page search (option 1 below), verified 2026-07-22.

**Reported from a SimpleShop backend session, 2026-07-22.** QA reported "Audit Trail: API call succeeds, but the UI shows no data." This is not a backend contract issue — the backend confirms `GET /audit-logs` returns data correctly. The likely root cause was traced into this repo's own code below; a frontend session should verify and fix.

**Reconfirmed 2026-07-22** with a fresh sample response — matches the same shape/limitation described below:

```json
{
  "items": [
    {
      "id": "019f88ff-34b5-753f-8043-86cb561dcc3a",
      "rootEntityType": "Product",
      "rootEntityId": "019f88d8-f7cd-7dbc-a0bb-8d59e2f95528",
      "service": "Product",
      "correlationId": "cf17eb9b-9c92-44b2-ae9d-e5fd9873ea04",
      "timestamp": "2026-07-22T08:02:28.56Z"
    },
    {
      "id": "019f881c-cb8f-74db-93d1-8e54b777202a",
      "rootEntityType": "Product",
      "rootEntityId": "019f881c-ba36-7cb6-bbf3-511d78067391",
      "service": "Product",
      "correlationId": "ec346127-5ebc-461f-8300-17610a3b3163",
      "timestamp": "2026-07-22T04:36:52.072Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 50,
  "totalCount": 2,
  "hasNextPage": false,
  "hasPreviousPage": false,
  "totalPages": 1
}
```

## Root cause (traced, not yet fixed)

`src/shared/entity/AuditTrailDialog.tsx:24-30`:

```ts
const { data, isLoading } = useQuery({
  queryKey: ['audit', service, entityId],
  queryFn: () => listAuditLogs({ service, pageSize: 50 }),
  enabled: open,
});

const entries = (data?.items ?? []).filter((entry) => entry.rootEntityId === entityId);
```

This fetches only the **50 most recent audit log entries for the whole `service`** (not entity-scoped — `ListAuditLogs` has no server-side entity-id filter, see `docs/backend/audit/README.md`), then filters client-side by `rootEntityId`. There is no fallback to fetch page 2+ if the target entity isn't in that first page.

**Failure mode:** once a service (e.g. "Product") has logged more than 50 audit events total, any entity whose most recent change is older than the 50 most-recent-across-the-service events becomes permanently invisible in this dialog — the API call succeeds (200, real data), `data.items` is non-empty, but `entries` is `[]` because the one row that matters isn't in the window. This matches the reported symptom exactly (success response, empty render) and would get worse over time as more services accumulate audit history.

This is a known, already-documented limitation — `docs/backlog.md` §5 flags "a real audit-log viewer page was never built" and references this exact client-filter approach — but it's tracked there as a missing _feature_ (dedicated `/audit` page), not as this specific _regression risk_ on the existing dialog. Worth cross-referencing both when picking this up.

## Fix applied

Took direction 1 (cheapest patch), plus direction 3 (distinct empty states):

- `src/shared/entity/AuditTrailDialog.tsx` — `fetchEntityAuditEntries()` now walks up to `MAX_PAGES_SEARCHED = 5` pages, stopping early on a match or on `hasNextPage === false`. Returns a `truncated` flag when it exhausts the page budget without finding the entity or reaching the end of the service's log.
- Empty state now reads `t('emptyTruncated')` instead of `t('empty')` when `truncated` is true, so "genuinely no history" and "gave up searching" no longer look identical.
- `src/i18n/messages/en/entity.json` — added the `emptyTruncated` copy.

Verified: `tsc --noEmit` and `eslint` both clean on the touched file.

Direction 2 (server-side `entityId` filter on `GET /audit-logs`) is still the real long-term fix and remains open — tracked in `docs/backlog.md` §5 (dedicated `/audit` page work). This client-side patch just removes the silent-failure regression risk in the meantime.

## Reference

- `src/shared/entity/AuditTrailDialog.tsx` (the dialog)
- `src/services/audit/list-audit-logs.ts` (the client, `AuditLogSummaryResponse.rootEntityId: string | null`)
- `docs/backend/audit/README.md` (backend contract, confirms no entity-id filter exists server-side)
- `docs/backlog.md` §5 (existing, related "dedicated /audit page" backlog item)
