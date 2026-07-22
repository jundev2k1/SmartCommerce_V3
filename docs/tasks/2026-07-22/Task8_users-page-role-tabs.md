# Task 8: Users page — split into Admin / Normal User tabs

**Status:** Blocked on a backend change — no list/search endpoint exists on the User service.

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

## Why this is blocked, not just a frontend task

There is currently no way to enumerate "all admin accounts" or "all user accounts" at all — the User service has no list/search endpoint (already documented in `docs/backlog.md`: _"User service has no list/search/delete"_). The two-tab UI described in the ask cannot be built against the current contract regardless of how the frontend is structured.

## What's needed to unblock

A backend change: a paginated list/search endpoint on the User service (e.g. `GET /profiles` or `/profiles/search`), ideally filterable by role. Also needs a decision on where the "Root may grant Admin or User; Admin may only grant User" visibility rule (already enforced server-side for _granting_ roles, per `docs/backend/user/README.md`) should apply to _viewing_ — should the list endpoint itself scope results by the caller's role, or should the frontend apply that filtering client-side after fetching everything?

## Suggested frontend shape once unblocked

- Two tabs (Admin / Normal User) using the existing `AppDataTable`/tab primitives already used elsewhere in this codebase.
- Root's own account appears in the Admin tab (per the ask) — confirm whether the new endpoint's role-scoping already handles this or whether the frontend needs to explicitly include the caller's own profile in that tab.
- Normal User tab gets real search (likely via the same `AppSearchBox`/`FilterPanel` pattern used by other entity lists) once the endpoint supports query params for it.

## Status

Blocked. No corresponding backend task exists yet for a User-service list/search endpoint — recommend raising one; this task can't proceed past design until that lands.
