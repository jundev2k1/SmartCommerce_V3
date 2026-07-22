# 0011 — Local Dialogs by Default, Zustand-Backed Registry for Cross-Cutting Modals

**Status:** Accepted

**Date:** 2026-07-20

## Context

Most modals in an admin dashboard are local to a feature (a "create product" dialog triggered by a button on the products page) and need no cross-cutting infrastructure. But a few modals are inherently cross-cutting — most notably, a notification arriving via SignalR ([0007](./0007-signalr-strategy.md)) that needs to trigger a modal from outside the component tree that owns it, which a purely local `useState`/`useDisclosure` pattern can't reach.

## Decision

Local, feature-scoped modals use `shared/ui`'s `Dialog` wrapper plus a `useDisclosure` hook — no global infrastructure. Cross-cutting modals (triggerable from outside their "home" component tree) go through a small Zustand-backed modal registry in `shared/stores/ui.store.ts`, keyed by modal id.

## Rationale

- Defaulting to local state for the common case (most modals) avoids over-engineering every dialog with global registry overhead it doesn't need.
- The registry exists specifically for the minority case where a modal must be opened from code that isn't a descendant of the modal's own component (e.g. a global notification handler triggering a detail modal) — prop-drilling a callback that deep, or duplicating modal-open state in multiple places, is worse than one small registry.
- This is a UI-state concern, correctly owned by Zustand per [0006](./0006-state-management-split.md) — the registry never holds the modal's _content_ data (e.g. the product being edited), only which modal id is open; content still comes from TanStack Query/props as normal.

## Alternatives considered

- **Global modal registry for every modal, always**: rejected — unnecessary indirection for the common local case, and makes a simple confirm-dialog harder to trace than a plain `useState`.
- **Context-based modal manager instead of Zustand**: rejected — Zustand is already the project's answer for cross-route client state ([0006](./0006-state-management-split.md)); introducing Context for this one case adds a second pattern for the same kind of problem with no real benefit.

## Consequences

- A feature only reaches for the registry when it actually needs cross-tree triggering — most CRUD modals never touch `ui.store.ts`.
- The registry stores modal _identity_ (which one is open, and minimal params like an id), never fetched data — components still fetch their own content via the normal query hooks once they know which modal/id is active.

## Related documents

[state/zustand-strategy.md](../state/zustand-strategy.md), [frontend/ui-components.md](../frontend/ui-components.md), [decisions/0007-signalr-strategy.md](./0007-signalr-strategy.md)
