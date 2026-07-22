# Authentication

**Purpose:** Login, registration, session restoration, logout, and route guarding for the whole admin app.

**Scope:** `features/auth` and the `(auth)`/`(admin)` route-group wiring that depends on it.

**Related documents:** [modules/overview.md](./overview.md), [roadmap.md](../roadmap.md), [backend/auth/README.md](../backend/auth/README.md), [backend/user/README.md](../backend/user/README.md), [decisions/0005-api-layer-and-auth.md](../decisions/0005-api-layer-and-auth.md), [decisions/0013-shared-shell-callback-props.md](../decisions/0013-shared-shell-callback-props.md)

**When to read:** Touching login/register/logout, session restoration, or route protection.

**When to ignore:** Working in an unrelated module that only needs to _know_ a session exists (those should read `shared/stores/session.store.ts` directly, not this doc).

---

## Feature folder(s)

`src/features/auth/`

## Data model (real backend contract)

`LoginRequest{email,password}`, `RegisterRequest{email,password,firstName,lastName,phoneNumber}` — see `src/services/auth/`. "Current user" comes from the **User** service's `GetUserDetail` (`GET /profiles/current/detail`), not Auth — Auth has no "me" endpoint. See `src/services/user/get-user-detail.ts`.

## Key flows

- **Login**: `LoginForm` → `useLoginMutation` → `login()` (sets HTTP-only cookies) → invalidate the current-user query → redirect to `/`.
- **Register**: `RegisterForm` → `useRegisterMutation` → `register()` — its own contract confirms it sets the _same_ session cookies login does, so a successful register redirects straight to `/`, not to `/login`.
- **Session restoration**: `useCurrentUserQuery` (`getUserDetail`, `retry: false`) is read by both `AuthGuard` and `GuestGuard`. On success, `session.store.ts` is populated via `setSession`; on any error, treated as "not authenticated."
- **Logout**: `useLogoutMutation` calls `logout()`; its `onSettled` (not `onSuccess`) clears the session store, drops the current-user query, and redirects to `/login` regardless of whether the network call itself succeeded — logout must work client-side even if the request fails.
- **Route protection**: `AuthGuard` wraps `(admin)` routes (redirects to `/login` if session check fails); `GuestGuard` wraps `(auth)` routes (redirects to `/` if already authenticated). Both are plain reusable wrapper components — any future route group needing the same protection uses the same two components.

## Dependencies on other modules

Depends on **User Management**'s backend contract (`GetUserDetail`) for "current user" data — a cross-service dependency, not a cross-feature one (no import from `features/users`).

## Deviations from standard conventions

`shared/layout`'s `AdminShell`/`Topbar`/`UserMenu` gained an optional `onLogout` callback prop to stay wired to this feature without violating the `shared/` dependency direction — see [decisions/0013-shared-shell-callback-props.md](../decisions/0013-shared-shell-callback-props.md). `shared/forms/FormField.tsx` also gained `aria-invalid`/`aria-describedby` wiring, needed for real accessibility on these forms — a small, additive fix, not a redesign.

## Open questions / backend dependencies

- **No `middleware.ts`.** Route protection is client-side only (via the guards above) — there's a brief loading state while the session check resolves rather than a server-redirect before any render. Acceptable for now; edge-level protection (checking cookie presence in Next.js middleware) is a reasonable future enhancement, not required by this phase.
- **401 vs 403 pages exist (`/401`, `/403`) but nothing in the current contract triggers `/403`** — no endpoint used by Auth/User documents role-gated 403 responses. Built because the phase asked for them; not yet exercised by any real scenario.
- **Auth's Bearer-vs-cookie ambiguity** (see [backend/auth/README.md](../backend/auth/README.md)) — this module implements cookie auth only, per ADR 0005.
