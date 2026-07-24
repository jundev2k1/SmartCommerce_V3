# User Management

**Purpose:** Manage admin/customer user profiles — create, view, and edit.

**Scope:** `features/users` and the `(admin)/users` route.

**Related documents:** [modules/overview.md](./overview.md), [roadmap.md](../roadmap.md), [backend/user/README.md](../backend/user/README.md), [backend/coverage-report.md](../backend/coverage-report.md), [backend/audit/README.md](../backend/audit/README.md)

**When to read:** Touching user CRUD, the Admin/Normal-User tabs, or wondering why Delete isn't wired up.

**When to ignore:** Any other module.

---

## Feature folder(s)

`src/features/users/`

## Data model (real backend contract)

`CreateUserRequest{email,userName,phoneNumber,firstName,lastName,roles?,tempPassword?}`, `UpdateUserRequest{firstName,lastName,phoneNumber}`, `GetUserDetailResponse`/`SearchUsersItemResponse` (both include `status: UserStatus` — the one enum in this whole integration with a **confirmed** value mapping: Active=1/Inactive=2/Suspended=3 — and `roles: string[] | null`). See `src/services/user/`.

## Key flows

- **List**: `UsersPage` renders two `AppTabs` (Admin / Normal User), each backed by its own `useUsersSearchQuery` call against real `POST /users/search` — independent search box and pagination per tab. Admin tab filters `role != 'User'` (so Root shows there too, matching the ask); Normal User tab filters `role == 'User'`.
- **Create**: toolbar `CreateButton` → `UserFormDialog` (create mode) → `CreateUserForm` → real `POST /profiles`.
- **Edit**: table row's pencil action → `UserFormDialog` (edit mode), pre-filled from the row data already in hand → real `PUT /profiles/{userId}`.
- **Audit trail**: table row's shield action → `AuditTrailDialog` → `listAuditLogs({ service: 'User', pageSize: 50 })` from `@/services/audit`, filtered client-side to `rootEntityId === userId`.
- **Delete**: rendered as a disabled button with an explanatory tooltip — no `DELETE /profiles/{userId}` endpoint exists, this is not wired to anything.

## `roles` is a write-once snapshot, not live

`SearchUsersItemResponse.roles` is denormalized onto the user profile at creation time (SimpleShop `docs/tasks/2026-07-22/Task4_search-users-endpoint-already-exists.md`, option 2). There is no role-change endpoint anywhere in the backend yet, so this is a non-issue today — but a future role-change elsewhere would **not** retroactively update rows already indexed this way. The Users page shows a one-line note above the tabs saying so.

## Roles are not exposed in the create form

`CreateUserRequest.roles` exists in the contract, but there's no "list valid roles" endpoint to build a picker against, so the field is omitted from the UI entirely rather than free-texted or guessed — the backend applies its own default when `roles` isn't sent.

## Dependencies on other modules

Reads the current user from **Authentication**'s `useCurrentUserQuery` (via its public barrel, not an internal import) rather than re-fetching. Reads from the **Audit** service directly for the audit-trail dialog — a service-layer dependency, not a feature dependency (Audit's own frontend module doesn't exist until Phase 11).

## Open questions / backend dependencies

- Whether User-service actions actually log under `service: "User"` in the Audit service is an assumption, not confirmed — the audit filter (`service: 'User'`) may return zero matches even where activity exists if the real value differs.
- `ListAuditLogs` has no entity-id filter, so the audit dialog's `pageSize: 50` fetch could miss older matching entries if a user has more than 50 recent audit events across _all_ users logged under `service: "User"` in that window.
