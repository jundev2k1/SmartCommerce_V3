# Task 8: Users page — split into Admin / Normal User tabs

**Status:** Implemented 2026-07-24. `POST /users/search` already existed; the backend added the missing `Roles` field (SimpleShop `docs/tasks/2026-07-22/Task4_search-users-endpoint-already-exists.md`, option 2/denormalize). See "Backend contract, as of 2026-07-24" below for the shape. Frontend tab-splitting is now built — see "Status" at the bottom.

## Ask

Change the Users page to:

- A tab splitting the user list into two views.
- **Admin tab:** all admin-role accounts (Root sees itself here too).
- **Normal User tab:** all user-role accounts — searchable and editable.

## Current state

`src/features/users/components/UsersPage.tsx`:

- `:23` — `const { data: currentUser } = useCurrentUserQuery();`
- **`:60` — `data={currentUser ? [currentUser] : []}`** — the entire "list" fed to the table is a **single-element array containing only the logged-in user's own profile**. `page={1}`, `pageCount={1}` (:61-62) are hardcoded.
- **No role-based filtering logic exists anywhere in this feature** — no `AppRole` import, no `.filter(...)` by role (confirmed via grep). The page doesn't actually show "same-role users"; the current behavior is narrower than reported — it shows exactly one row, period.
- Data-source comment confirms why: `src/features/users/api/users.queries.ts:13-16` — _"No list endpoint exists (see docs/backend/user/README.md), so this is the only read hook beyond the current-user query."_ `src/services/user/` only has `create-user.ts`/`get-user.ts`/`get-user-detail.ts`/`update-user.ts` — no list/search file, confirming the backend genuinely doesn't expose one.
- `src/app/(admin)/users/page.tsx` is a trivial wrapper with no additional logic.

## Why this is blocked, not just a frontend task (corrected)

`POST /users/search` is real, admin-only, paginated/filterable/sortable, confirmed from backend source (`User.API/Endpoints/SearchUsers.cs`) — not a Swagger artifact. So enumerating "all users" is no longer the blocker. What's still missing: **the search response has no `Roles` field**, and there's no way to filter/sort by role either — roles live in Auth's per-user cache (`RoleCacheReader`), and no batch/multi-user role lookup exists anywhere in the backend today. Splitting into Admin/Normal-User tabs needs to know each row's role, which this endpoint genuinely can't give you yet. Full investigation + three unblocking options (fan-out after paging / denormalize onto the profile / punt to client-side bucketing): SimpleShop `docs/tasks/2026-07-22/Task4_search-users-endpoint-already-exists.md`.

Also still open, unaffected by the correction above: there's no frontend client function for `/users/search` at all yet (`src/services/user/` has no `search-users.ts`) — that part of the work is unblocked and could start once the `Roles` decision is made, or even before it if a first pass without role-based tabs is acceptable as an interim step.

## Backend contract, as of 2026-07-24

`SearchUsersItemResponse` now has a `roles: string[]` field on every row. `UserCriteriaDefinition` also gained a `role` filter field, `eq`/`ne` only (not sortable — role is multi-valued) — e.g. `{ "field": "role", "operator": "eq", "value": "Admin" }` gives real server-side pagination per tab, not just client-side bucketing after fetching everything. Caveat carried over from the backend task: `Roles` is a write-once snapshot taken at profile-creation time, not live — there is no role-change endpoint anywhere in the backend yet, so this is a non-issue today, but don't assume a role edit elsewhere would retroactively update search results.

Still open: no decision was made on where the "Root may grant Admin or User; Admin may only grant User" visibility rule (already enforced server-side for _granting_ roles) should apply to _viewing_ — the search endpoint does not scope results by the caller's own role today, so an Admin caller can see other Admins/Roots in results same as Root can. Decide whether that needs frontend-side filtering or a backend follow-up before shipping the tabs.

## Suggested frontend shape once unblocked

- Two tabs (Admin / Normal User) using the existing `AppDataTable`/tab primitives already used elsewhere in this codebase.
- Root's own account appears in the Admin tab (per the ask) — confirm whether the endpoint's role-scoping already handles this or whether the frontend needs to explicitly include the caller's own profile in that tab.
- Normal User tab gets real search via `POST /users/search`'s existing `keyword`/`filters`/`sorts` support (likely via the same `AppSearchBox`/`FilterPanel` pattern used by other entity lists) — this part doesn't need the `Roles` decision at all.

## Status

Implemented 2026-07-24:

- `src/services/user/search-users.ts` — `searchUsers(request)` client function for `POST /users/search`, `SearchUsersItemResponse` including `roles: string[] | null`.
- `src/features/users/api/users.queries.ts` — `useUsersSearchQuery({ roleTab, search, page, pageSize })`. `roleTab: 'admin'` sends `{ field: 'role', operator: 'ne', value: 'User' }` (matches Root + Admin in one server-side query, satisfying "Root sees itself here too" without a second request); `roleTab: 'user'` sends `{ field: 'role', operator: 'eq', value: 'User' }`. Both paginate server-side. `useCreateUserMutation`/`useUpdateUserMutation` now invalidate the search cache too.
- `src/features/users/components/UsersPage.tsx` — rewritten around `AppTabs` (Admin / Normal User), each tab backed by its own `UserRoleTable` with independent search/page state so switching tabs doesn't reset the other. Added a `roles` column (badges) so Root vs Admin is visible within the merged Admin tab. Edit/audit-trail/delete-disabled row actions and the create dialog are unchanged, just wired to the new per-tab data instead of the single-row `currentUser` array.
- Deliberately did **not** add frontend-side filtering to keep Root out of an Admin's view — the ask explicitly wants "all admin-role accounts (Root sees itself here too)" merged into one tab, matching how the backend already returns results unscoped by caller role.
- i18n: replaced the now-inaccurate `listLimitationNote` with `rolesSnapshotNote` (roles are a write-once creation-time snapshot, not live) plus `tabs.admin`/`tabs.user`/`table.roles`/`searchPlaceholder`.
- Verified: `tsc --noEmit` and `eslint` clean. No live backend in this sandbox to click-through end-to-end — verify real pagination/role-filtering against a live backend before shipping.
