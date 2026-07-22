# 0002 — App Router Is Thin Routing Only

**Status:** Accepted

**Date:** 2026-07-20

## Context

Given [0001](./0001-feature-first-architecture.md) established that business logic lives in `src/features/`, we need an explicit rule for what's still allowed inside `src/app/`, otherwise logic creeps back into route folders over time (this is a common regression in Next.js App Router codebases even after teams intend to keep pages thin).

## Decision

`src/app/` may only contain: layouts, pages, loading/error/not-found boundaries, route segment config, metadata exports, and a single import + render of a feature's exported screen component. No data fetching, form logic, or business state may be written directly inside `app/`.

## Rationale

- Makes the "where does this code go" question mechanical: if it's not one of the listed things, it doesn't belong in `app/`.
- Keeps route restructuring cheap — moving a URL means editing one thin file, not extracting logic first.
- Global providers (theme, query client, i18n) have exactly one legitimate home (`app/layout.tsx`), preventing provider sprawl.

## Alternatives considered

- **No explicit rule, rely on code review discipline**: rejected — without a written rule, "just this once" exceptions accumulate silently across many sessions/contributors over a multi-year project.

## Consequences

- Every page file is expected to be near-trivial (a handful of lines). A page that's growing is a signal its logic belongs in the feature instead.
- Route-level `loading.tsx`/`error.tsx` boundaries handle only initial-load and unexpected-error states; in-page loading/error states are TanStack Query's responsibility — see [services/error-handling.md](../services/error-handling.md).

## Related documents

[frontend/routing.md](../frontend/routing.md), [architecture/overview.md](../architecture/overview.md)
