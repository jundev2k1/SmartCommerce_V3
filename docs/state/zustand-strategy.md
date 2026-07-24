# Zustand Strategy

**Purpose:** Concrete rules for what goes into a Zustand store and how stores are organized.

**Scope:** Zustand only. For the boundary against TanStack Query, see [overview.md](./overview.md).

**Related documents:** [overview.md](./overview.md), [query-strategy.md](./query-strategy.md), [decisions/0006-state-management-split.md](../decisions/0006-state-management-split.md), [decisions/0011-modal-strategy.md](../decisions/0011-modal-strategy.md)

**When to read:** Adding any cross-component/cross-route client state.

**When to ignore:** The state in question is server data (→ TanStack Query) or scoped to one component (→ local state).

---

## Global stores (`src/shared/stores/`)

- **`session.store.ts`** — minimal auth session cache (user id, roles, display name) written after login/refresh; read by nav and route guards. Not the source of truth for the full user profile — that's a TanStack Query-fetched value if/when needed in full.
- **`theme.store.ts`** — only if something beyond next-themes' own state is needed (e.g. persisting a "reduce motion" preference); the light/dark/system value itself is owned by next-themes, not duplicated here — see [frontend/theming.md](../frontend/theming.md).
- **`ui.store.ts`** — sidebar collapsed/expanded, and the global modal registry (see [decisions/0011-modal-strategy.md](../decisions/0011-modal-strategy.md)).
- **`local-orders.store.ts`** (Phase 4 "Client Mock") — tracks the ids of orders created from this browser, since the Order service has no list/search endpoint. Not a cache of order _data_ (that stays in TanStack Query via `useOrderQuery`) — only the set of ids "My Orders" should fetch. See [modules/client-mock.md](../modules/client-mock.md). (Warehouse/Inventory had the same "no list endpoint" workaround in Phase 5 — `local-warehouses.store.ts`/`local-inventory-ids.store.ts` — retired once `/warehouses/search`/`/inventories/search`/`/inventory-transactions/search` landed; see [modules/inventory-management.md](../modules/inventory-management.md).)
- **`notifications.store.ts`** (Phase 1 scaffold, populated in Phase 7 "Notification Center") — unread notification count for the topbar bell badge, kept current via SignalR events and mutation side-effects (see [realtime/signalr-strategy.md](../realtime/signalr-strategy.md)); the full notification list itself stays in TanStack Query. Also persists `readIds` — ids this browser has explicitly marked read — since the Notification list DTO has no read/unread field and the `NotificationStatus` enum is opaque/unpublished (see [modules/notification-center.md](../modules/notification-center.md)); "unread" is computed as "loaded and not in this set," not read from the backend directly.
- **`signalr.store.ts`** — the hub connection's status (`disconnected`/`connecting`/`connected`), updated by the SignalR manager's lifecycle callbacks. Genuinely client-only, not a server-data cache.
- **`locale.store.ts`** — current locale. Minimal by design: next-intl owns message resolution, this store just tracks which locale is active so non-React code (e.g. the Axios client setting an `Accept-Language` header) can read it without a hook.

## What's deliberately NOT a Zustand store

**Theme** is not duplicated here even though it's cross-route client state — `next-themes` already owns and persists it, and [decisions/0009-theme-strategy.md](../decisions/0009-theme-strategy.md) explicitly rejects mirroring it into Zustand to avoid a second source of truth for the same value. Any future theme-_adjacent_ preference that `next-themes` doesn't itself track (e.g. a "reduce motion" toggle) is a legitimate candidate for `ui.store.ts` — the theme value itself never is.

## Feature-local stores (`features/<name>/store/`)

Rare. Only create one when a feature has state that must persist across more than one of its own components/routes and doesn't fit any global store above. Most features need zero stores — don't add one speculatively.

## Store shape convention

```ts
// shared/stores/ui.store.ts
import { create } from 'zustand';

interface UiState {
  sidebarCollapsed: boolean;
  toggleSidebar: () => void;
}

export const useUiStore = create<UiState>((set) => ({
  sidebarCollapsed: false,
  toggleSidebar: () => set((s) => ({ sidebarCollapsed: !s.sidebarCollapsed })),
}));
```

One `create()` call per store file, hook name matches file name per [conventions/naming.md](../conventions/naming.md).

## What never goes in a Zustand store

Anything returned by a `.service.ts`/`.queries.ts` call. If you find yourself calling `setState` inside a `useEffect` that watches a query result "to keep it globally accessible," stop — export the query hook itself from the feature's barrel instead, or lift the query to a shared location; don't fork the data into a second store.
