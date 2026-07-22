# User Management

**Purpose:** Manage admin/customer user profiles — create, view, and edit.

**Scope:** `features/users` and the `(admin)/users` route.

**Related documents:** [modules/overview.md](./overview.md), [roadmap.md](../roadmap.md), [backend/user/README.md](../backend/user/README.md), [backend/coverage-report.md](../backend/coverage-report.md), [backend/audit/README.md](../backend/audit/README.md)

**When to read:** Touching user CRUD, or wondering why the Users page doesn't have a real list/search/delete.

**When to ignore:** Any other module.

---

## Feature folder(s)

`src/features/users/`

## Data model (real backend contract)

`CreateUserRequest{email,userName,phoneNumber,firstName,lastName,roles?,tempPassword?}`, `UpdateUserRequest{firstName,lastName,phoneNumber}`, `GetUserDetailResponse` (includes `status: UserStatus` — the one enum in this whole integration with a **confirmed** value mapping: Active=1/Inactive=2/Suspended=3). See `src/services/user/`.

## Key flows

- **Create**: toolbar `CreateButton` → `UserFormDialog` (create mode) → `CreateUserForm` → real `POST /profiles`. Success toast explicitly notes the new user won't appear in the table (see limitation below).
- **Edit**: table row's pencil action → `UserFormDialog` (edit mode), pre-filled from the row data already in hand → real `PUT /profiles/{userId}`.
- **Audit trail**: table row's shield action → `UserAuditTrailDialog` → `listAuditLogs({ service: 'User', pageSize: 50 })` from `@/services/audit`, filtered client-side to `rootEntityId === userId`.
- **Delete**: rendered as a disabled button with an explanatory tooltip — see the limitation below, this is not wired to anything.

## ⚠️ Known constraint: the table has exactly one real row

**The User backend contract has no list, search, filter, or delete endpoint** — only Create, Get-by-id, Get-current-detail, and Update (confirmed in [backend/coverage-report.md](../backend/coverage-report.md); this doc previously missed calling that gap out explicitly, corrected in this phase). Given that, and per the explicit direction to placeholder rather than invent behavior:

- The `AppDataTable` on the Users page is fed exactly one row: the current authenticated user, from the same `useCurrentUserQuery` result the Auth module already fetches for session restoration (no extra call). This is real data, not a mock — it's simply the only user this contract lets us enumerate.
- Search, pagination, and filter controls are **not rendered at all** — a non-functional search box would be more misleading than omitting one. A one-line note above the table states the limitation plainly.
- Delete renders disabled with a tooltip ("Not available — the backend doesn't support deleting users yet") rather than being hidden, so it's a visible TODO, not a silent gap.
- A newly created user has no way to appear anywhere in this UI (there's no way to look it up without knowing its id) — the create-success toast says so directly.

**When the backend adds a list (and ideally delete/search) endpoint**, this module should be revisited to build a real paginated table, remove the limitation note, enable Delete, and add search/filter controls — none of the surrounding architecture (dialogs, forms, mutations) needs to change, only the data-fetching side.

## Roles are not exposed in the create form

`CreateUserRequest.roles` exists in the contract, but there's no "list valid roles" endpoint to build a picker against, so the field is omitted from the UI entirely rather than free-texted or guessed — the backend applies its own default when `roles` isn't sent.

## Dependencies on other modules

Reads the current user from **Authentication**'s `useCurrentUserQuery` (via its public barrel, not an internal import) rather than re-fetching. Reads from the **Audit** service directly for the audit-trail dialog — a service-layer dependency, not a feature dependency (Audit's own frontend module doesn't exist until Phase 11).

## Open questions / backend dependencies

- Whether User-service actions actually log under `service: "User"` in the Audit service is an assumption, not confirmed — the audit filter (`service: 'User'`) may return zero matches even where activity exists if the real value differs.
- `ListAuditLogs` has no entity-id filter, so the audit dialog's `pageSize: 50` fetch could miss older matching entries if a user has more than 50 recent audit events across _all_ users logged under `service: "User"` in that window.
