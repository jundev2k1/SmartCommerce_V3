# SignalR Realtime Strategy

**Purpose:** How a single SignalR hub connection is managed and how realtime events feed into the rest of the state architecture.

**Scope:** Realtime transport only. Notification-feature-specific UI belongs to `docs/modules/notifications.md` once that module's phase starts (see [modules/overview.md](../modules/overview.md)).

**Related documents:** [state/query-strategy.md](../state/query-strategy.md), [state/zustand-strategy.md](../state/zustand-strategy.md), [decisions/0007-signalr-strategy.md](../decisions/0007-signalr-strategy.md), [roadmap.md](../roadmap.md) (Phase 10)

**When to read:** Building any realtime feature (notification center, live order/inventory updates).

**When to ignore:** Any task with no live/push-update requirement — most CRUD features don't need this doc.

---

## Single connection

One SignalR hub connection, managed in `shared/lib/realtime/signalr-client.ts`, its lifecycle tied to the auth session (connect after login, disconnect on logout) — not one connection per feature.

```ts
// shared/lib/realtime/signalr-client.ts
export const hubConnection = new signalR.HubConnectionBuilder()
  .withUrl(`${env.NEXT_PUBLIC_API_BASE_URL}/hubs/realtime`, { withCredentials: true })
  .withAutomaticReconnect()
  .build();
```

## Feature hooks subscribe, they don't own state

A feature that needs realtime updates uses the shared `useHubEvent` hook (`shared/lib/realtime/useHubEvent.ts`, added Phase 6) rather than importing `signalr-client` directly, and on receipt pushes the update into the TanStack Query cache — via `queryClient.setQueryData` for a targeted update, or `invalidateQueries` for a simple refetch — rather than storing the event payload in a Zustand store.

```ts
// shared/lib/realtime/useHubEvent.ts — generic, reusable by any feature
export function useHubEvent<T>(event: string, handler: (payload: T) => void, enabled = true) {
  useEffect(() => {
    if (!enabled) return;
    on<T>(event, handler);
    return () => off(event, handler as (...args: unknown[]) => void);
  }, [event, handler, enabled]);
}

// features/orders/hooks/useOrderRealtimeUpdates.ts — a concrete feature hook built on it
export function useOrderRealtimeUpdates(onEvent?: (event: OrderStatusChangedEvent) => void) {
  const queryClient = useQueryClient();
  const handler = useCallback(
    (event: OrderStatusChangedEvent) => {
      queryClient.invalidateQueries({ queryKey: orderKeys.detail(event.orderId) }); // targeted — no whole-list refetch
      onEvent?.(event);
    },
    [queryClient, onEvent],
  );
  useHubEvent('OrderStatusChanged', handler);
}
```

This keeps TanStack Query as the single source of truth for this data (per [state/overview.md](../state/overview.md)) — SignalR is just another way that cache gets updated, alongside normal query refetches and mutation invalidation. Always invalidate the narrowest key that covers the changed entity (a single order's detail query, not every order query) — see [modules/order-management.md](../modules/order-management.md) for why that's already the minimal invalidation in a module with no monolithic "list" query to begin with.

## Reconnection & auth

`withAutomaticReconnect()` handles transient network drops. If the underlying auth cookie session ends (refresh-token flow fails, per [services/api-layer.md](../services/api-layer.md)), the hub connection is explicitly stopped and restarted after the next successful login — it does not attempt to silently keep pushing events for a logged-out session.

**Connection lifecycle is wired** (Phase 6, ahead of Phase 10): `features/auth/components/AuthGuard.tsx` calls `connect()` once a session is confirmed; `features/auth/api/auth.queries.ts`'s `useLogoutMutation` calls `disconnect()` in `onSettled`. Both are fire-and-forget (`.catch(() => undefined)`) — a failed connection attempt (expected in any environment without a live backend/hub) must never block or break the auth flow itself.

## Event catalog

Still not defined by any of the 7 backend services — confirmed again in Phase 6 (the Notification service, the only one that could plausibly expose push events, is REST-only with no hub endpoints documented; see [backend/notification/README.md](../backend/notification/README.md)). Do not invent event contracts. Phase 6 proved out the full subscribe → targeted-invalidate → cache-update pattern against a placeholder event (`'OrderStatusChanged'`, see [modules/order-management.md](../modules/order-management.md)) rather than waiting for Phase 10 to do this for the first time — mock/simulate events locally against this same pattern for any future realtime feature until the real contract exists; swapping in real event names/payloads later is a one-line change per feature hook, not a rearchitecture.

**Phase 7 (Notification Center)** reused this exact infrastructure unchanged — `useHubEvent` and the connect/disconnect lifecycle wired in Phase 6 needed no modification. `features/notifications/hooks/useNotificationRealtimeUpdates.ts` subscribes to another placeholder event, `'NotificationCreated'`, and on receipt calls `queryClient.setQueryData` to prepend the new item directly into the cached first page of the notification feed (rather than `invalidateQueries`, since the shape is known and a targeted cache write avoids a refetch entirely) plus increments the unread counter in `shared/stores/notifications.store.ts`. This is the second confirmation that the subscribe → targeted-cache-update pattern generalizes cleanly across features without touching the shared abstraction.
