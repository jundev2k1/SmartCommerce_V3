# 0013 — Shared Shell Components Take Behavior via Callback Props

**Status:** Accepted

**Date:** 2026-07-20 (Phase 2)

## Context

Phase 1 built `shared/layout`'s `AdminShell`/`Topbar`/`UserMenu` with a static, unwired "Log out" menu item, explicitly deferred: "static placeholder — no real session yet." Phase 2 needed to wire it to a real `useLogoutMutation`, which lives in `features/auth` (it needs the session store, TanStack Query, and `next/navigation`). But `shared/` may never import from `features/` — [architecture/overview.md](../architecture/overview.md)'s dependency direction is one-way: `app → features → services → shared`.

## Decision

`AdminShell`, `Topbar`, and `UserMenu` each accept an optional `onLogout?: () => void` callback prop, threaded straight through (`AdminShell` → `Topbar` → `UserMenu`). The real behavior is supplied where `features/auth` and `shared/layout` are both allowed to be imported together: `app/(admin)/layout.tsx`, which wraps `AdminShell` in a small client component calling `useLogoutMutation()` and passing the result down.

## Rationale

- Preserves the existing dependency direction exactly — `shared/layout` stays generic and feature-agnostic; it doesn't know what "logout" _does_, only that something happens when the item is clicked.
- Reusable pattern: any future feature needing to inject behavior into shared shell chrome (e.g. a future "Switch workspace" action, or wiring `NotificationBell` to a real feature once Phase 10 lands) follows the same shape — an optional callback prop, wired at the `app/` composition layer.
- Minimal and additive: no folder moves, no new shared subdirectories, no required-prop breaking changes — components render exactly as before when the prop is omitted (`UserMenu`'s logout item is disabled if `onLogout` isn't supplied, rather than silently doing nothing).

## Alternatives considered

- **Move `UserMenu`/`Topbar`/`AdminShell` into `features/auth` or a new top-level layer**: rejected — these are cross-cutting shell chrome used by every admin route, not auth-specific; moving them would be a folder-structure change with no compensating benefit, and would need to happen again for every other feature that wants to touch shell chrome.
- **Have `shared/layout` call `@/services/auth` directly**: rejected — `shared/` can't import `services/` either (same dependency-direction rule), and baking one specific service call into shared chrome would make it impossible to reuse the same components for, e.g., a future non-cookie auth scheme without editing shared code.

## Consequences

- `AdminShell`/`Topbar`/`UserMenu`'s prop types gained one optional field each — call sites that don't pass `onLogout` are unaffected.
- Any future "inject feature behavior into shared shell chrome" need should follow this same callback-prop shape rather than reaching for a new pattern.

## Related documents

[architecture/overview.md](../architecture/overview.md), [architecture/folder-structure.md](../architecture/folder-structure.md), [modules/authentication.md](../modules/authentication.md)
