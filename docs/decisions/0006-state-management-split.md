# 0006 — TanStack Query Owns Server State, Zustand Owns Cross-Route Client State

**Status:** Accepted

**Date:** 2026-07-20

## Context

Using both TanStack Query and Zustand without an explicit ownership rule is a well-known source of bugs in React codebases: it's tempting to mirror a fetched list into a Zustand store "for easy global access," which then silently diverges from the query cache (stale after a mutation invalidates the query but not the mirrored store, or vice versa) — two sources of truth for the same data with no single place that's guaranteed correct.

## Decision

TanStack Query is the only owner of server state, including paginated and cursor-based lists. Zustand is reserved for client-only state that must survive across components/routes and is not server data. Local `useState`/RHF own everything scoped to a single component/form.

The one deliberate, narrow exception: a minimal auth session cache in Zustand (user id/roles), written once after login/refresh so route guards and nav can read it synchronously — explicitly documented as a _cache of_ a query result, not an independent source of truth.

## Rationale

- A single, explicit rule ("did this come from an API? → Query") is easy to apply consistently across 15+ features and many future sessions, without requiring case-by-case judgment calls that could drift over time.
- Avoids the stale-mirror bug class described above entirely, by construction.
- The auth-session exception is justified because route guards/nav need synchronous access before a suspending query resolves (e.g. to decide whether to redirect to login) — but it's a read-derived cache, not a place new server data should ever be written directly.

## Alternatives considered

- **Let each feature decide case-by-case**: rejected — leads to inconsistent choices across features/sessions on a project without a single team maintaining shared context.
- **Zustand for everything, no TanStack Query**: rejected — Zustand has no built-in cache invalidation, refetch-on-window-focus, or request deduplication; reimplementing those is exactly what TanStack Query already solves.

## Consequences

- Code review should flag any `setState` inside a `useEffect` watching a query result as a likely violation of this rule (see [state/zustand-strategy.md](../state/zustand-strategy.md)'s closing note).
- Cart state (Phase 7) is a genuine Zustand case, not an exception — it's client-only until checkout submits it.

## Related documents

[state/overview.md](../state/overview.md), [state/zustand-strategy.md](../state/zustand-strategy.md), [state/query-strategy.md](../state/query-strategy.md)
