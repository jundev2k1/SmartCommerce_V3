# State Management Overview

**Purpose:** Draw a hard line between what TanStack Query, Zustand, and local component state are each responsible for, so responsibilities never overlap.

**Scope:** Ownership boundaries only. Implementation detail for each tool is in [zustand-strategy.md](./zustand-strategy.md) and [query-strategy.md](./query-strategy.md).

**Related documents:** [zustand-strategy.md](./zustand-strategy.md), [query-strategy.md](./query-strategy.md), [decisions/0006-state-management-split.md](../decisions/0006-state-management-split.md)

**When to read:** Before adding any new piece of state — always ask "which of these three owns this?" before writing code.

**When to ignore:** Never skip this when introducing new state; it's short by design.

---

## The three owners

| State kind                                                   | Owner                       | Examples                                                                                                                          |
| ------------------------------------------------------------ | --------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| Anything that came from, or will be sent to, the backend     | **TanStack Query**          | product list, user profile, order history, notification feed                                                                      |
| Client-only state that must survive across components/routes | **Zustand**                 | theme selection's _derived_ UI needs, sidebar collapsed, cart contents, global modal registry, auth session cache                 |
| Client-only state scoped to one component/subtree            | **`useState`/`useReducer`** | a dialog's open/closed flag, a form's local draft before submit (once inside `shared/forms`, RHF owns this), hover/focus UI state |

## The one rule that prevents overlap

**If it's fetched from or persisted to an API, TanStack Query owns it — full stop, even if it feels like "just a cache."** Zustand must never hold a copy of server data (e.g. don't mirror a fetched product list into a Zustand store "for convenience") — this is the most common way these two tools end up fighting over the same source of truth. See [decisions/0006-state-management-split.md](../decisions/0006-state-management-split.md) for the incident-shaped rationale behind this rule.

The one deliberate exception: an **auth session cache** in Zustand — a minimal, derived snapshot (e.g. current user id/roles) written once after login/refresh so route guards and nav can read it synchronously without suspending on a query. It is a cache of a TanStack Query result, not an independent source of truth — see [zustand-strategy.md](./zustand-strategy.md).

## Quick decision flow

1. Does this data exist on a backend (now or once Phase 12 lands)? → TanStack Query.
2. Does it need to be read/written from more than one route or component subtree, and it's not server data? → Zustand.
3. Otherwise → local component state (or RHF state, if it's form input).
