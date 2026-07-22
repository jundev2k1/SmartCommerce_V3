# 0007 — Single SignalR Connection Feeding the Query Cache

**Status:** Accepted

**Date:** 2026-07-20

## Context

Realtime features (notification center now, potentially live order/inventory updates later) need a way to receive server-pushed events. Without a stated strategy, it would be easy for each realtime feature to open its own hub connection and maintain its own local state for received events — duplicating both the connection lifecycle and the "where does this data live" question that [0006](./0006-state-management-split.md) already answered for fetched data.

## Decision

One SignalR hub connection, lifecycle-managed in `shared/lib/realtime`, tied to the auth session. Feature-level hooks subscribe to specific events and push updates into the TanStack Query cache (`setQueryData`/`invalidateQueries`), never into a separate Zustand store.

## Rationale

- One connection avoids the overhead and complexity of multiple concurrent hub connections per browser tab.
- Routing realtime updates into the TanStack Query cache keeps [0006](./0006-state-management-split.md)'s rule intact: server-shaped data (even server-_pushed_ data) still has exactly one owner, the query cache — a realtime event is just another way that cache gets updated, alongside refetch and mutation invalidation.
- Tying connection lifecycle to auth session means realtime features don't need their own auth-awareness — they just assume the connection exists when logged in.

## Alternatives considered

- **Per-feature hub connections**: rejected — unnecessary overhead, and complicates the "single connection" mental model as more realtime features are added.
- **Store realtime event payloads in Zustand**: rejected — reintroduces the two-sources-of-truth problem [0006](./0006-state-management-split.md) exists to prevent, just via a different transport than a normal fetch.

## Consequences

- The real event catalog is unknown until the backend provides it — this decision fixes the _integration pattern_, not actual event names/payloads. See [api/README.md](../api/README.md).
- Phase 10 development simulates/mocks events locally against this same pattern, so swapping in the real hub later is a connection-URL and event-name change, not a rearchitecture.

## Related documents

[realtime/signalr-strategy.md](../realtime/signalr-strategy.md), [state/query-strategy.md](../state/query-strategy.md), [decisions/0006-state-management-split.md](./0006-state-management-split.md)
