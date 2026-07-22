# Backend: Audit Service

**Purpose:** What the Audit Service exposes and how the frontend's `src/services/audit` client maps to it.

**Scope:** This service's contract only. Cross-service concerns (envelope, auth, correlation ID) live in [services/api-layer.md](../../services/api-layer.md).

**Related documents:** [backend/README.md](../README.md), [backend/feature-mapping.md](../feature-mapping.md), [backend/coverage-report.md](../coverage-report.md), `src/services/audit/`

**When to read:** Building the Audit / System module (Phase 11), or anything reading audit log data.

**When to ignore:** Any work not touching audit logs.

---

## Purpose

Cross-service audit log — a read-only record of significant changes across the platform (who did what, when, from where), including a full before/after change tree per entry.

## Base path

`/api/audit` (behind the shared gateway — same origin as inventory/order/product/user per their Swagger `contact.url`).

## Auth requirements

Admin only, per both endpoints' descriptions. No distinct scopes/roles documented beyond "admin only" — enforcement detail is entirely server-side; the frontend just needs an authenticated admin session (cookie-based, see [services/api-layer.md](../../services/api-layer.md)).

## Endpoints

| Operation            | Method & Path                  | Client function           |
| -------------------- | ------------------------------ | ------------------------- |
| Get audit log detail | `GET /audit-logs/{auditLogId}` | `getAuditLog(auditLogId)` |
| List audit logs      | `GET /audit-logs`              | `listAuditLogs(params)`   |

## Request/response DTOs

Each endpoint file in `src/services/audit/` declares its own request/response DTOs inline (`get-audit-log.ts`, `list-audit-logs.ts`) — not re-pasted here; `src/services/audit/types/` holds the shared `AuditNode`/`AuditMetadata` shapes. Key shapes: `GetAuditLogResponse` (includes a recursive `AuditNode` change-tree + `AuditMetadata`), `AuditLogSummaryResponse` (list item).

## Pagination style

Page-based (`page`/`pageSize` query params), response wrapped in the standard `BackendPaginatedResult<AuditLogSummaryResponse>` (`items`/`pageNumber`/`pageSize`/`totalCount`/`hasNextPage`/`hasPreviousPage`/`totalPages`). No cursor pagination on this service.

## Error responses

Swagger only models the `200` response schema for both endpoints; non-200 behavior is documented only in prose (`GetAuditLog`: 404 if not found). No formal error-response schema exists to type against — treat any non-2xx as a normalized `ApiError` per [services/error-handling.md](../../services/error-handling.md).

## Known limitations / TODOs

- [ ] `AuditAction` (`0`, `1`, `2`) has no published name mapping — kept as opaque `number` in `types/audit-action.ts`. Confirm mapping with backend before building any UI that needs to label actions (e.g. "Created"/"Updated"/"Deleted").
- [ ] No documented error-response body shape for the 404 case — confirmed only by prose.
- [ ] `service` filter on `ListAuditLogs` is a free-text string match against originating service name (e.g. "Product", "Order") — no enum of valid values is published; the frontend can't offer a validated dropdown without one.
