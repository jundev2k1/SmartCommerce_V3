# 0010 — Page-Based Pagination by Default, Cursor Pagination for Feeds

**Status:** Accepted

**Date:** 2026-07-20

## Context

This admin dashboard has two distinct list shapes: bounded, sortable/filterable admin tables (products, users, orders) where users expect page numbers and jump-to-page behavior; and unbounded, append-only feeds (notifications, audit log) where "page 5 of 40" is meaningless UX and infinite scroll/"load more" is the natural interaction.

## Decision

Standard admin list tables use page-based pagination (`page`/`pageSize` params) via TanStack Table's manual pagination mode + TanStack Query with `keepPreviousData`. Feed-style lists use cursor-based pagination via `useInfiniteQuery`, wrapped in a shared `useCursorList` hook.

## Rationale

- Page-based pagination matches the mental model of a data table with sort/filter/jump-to-page controls, which is what TanStack Table is built for.
- `keepPreviousData` (formerly `keepPreviousData`/`placeholderData`) avoids UI flicker/layout shift when a user pages through a table — a small but noticeable UX detail across every list in the app given how many tables this dashboard has.
- Cursor pagination is the correct fit for feeds because the underlying data is frequently prepended to (a new notification/audit entry can arrive at any time) — offset-based paging over a frequently-mutating list causes skipped/duplicated items, which cursor pagination avoids by design.
- A shared `useCursorList` wrapper prevents every cursor-paginated feature from reimplementing `getNextPageParam` slightly differently.

## Alternatives considered

- **Cursor pagination everywhere, including admin tables**: rejected — TanStack Table's page-jump/sort UX assumes page-based semantics; forcing cursor pagination into that UI adds complexity for no benefit on bounded, rarely-mutated-during-viewing datasets.
- **Page-based pagination for feeds too**: rejected — feeds are frequently prepended to while being viewed, which page-based pagination handles poorly (see rationale above).

## Consequences

- New list features must decide up front which category they are (bounded table vs. append-only feed) — see [modules/_template.md](../modules/_template.md)'s "Key flows" section as the place to record that choice.
- Both patterns are documented once, centrally, in [state/query-strategy.md](../state/query-strategy.md) rather than being re-derived per feature.

## Related documents

[state/query-strategy.md](../state/query-strategy.md), [realtime/signalr-strategy.md](../realtime/signalr-strategy.md)
