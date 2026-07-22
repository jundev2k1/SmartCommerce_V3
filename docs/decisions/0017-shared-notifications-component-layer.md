# 0017 — `shared/notifications/`: Reusable Notification-Center Presentation

**Status:** Accepted

**Date:** 2026-07-20 (Phase 7 — Notification Center & Realtime)

## Context

The brief is explicit that the Notification module must be "the single place responsible for displaying realtime user notifications throughout the application" and that "business modules... should never implement notification UI themselves." That only holds if the presentational pieces (bell badge, item row, timestamp, icon, loading/empty states) are extracted somewhere every future consumer can reach — not buried inside one Topbar-specific component.

## Decision

Add `shared/notifications/`: `UnreadBadge`, `NotificationTimestamp`, `NotificationIcon`, `NotificationSkeleton`, `NotificationEmptyState`, `NotificationItem`, `NotificationList`, `NotificationActions`. All are prop-driven, no internal data fetching — the same rule `shared/commerce`/`shared/inventory` already follow. `features/notifications` owns every query/mutation/realtime subscription and composes these into the real `NotificationBell`/`NotificationDropdown`/`NotificationDetailDialog`.

## Rationale

- **Prop-driven, not self-fetching**, so any future surface (a full-page notification history, a widget inside another module) can reuse the same list/item/badge rendering without also inheriting `features/notifications`' specific query/cache wiring.
- **`NotificationItem`'s read/unread state is a prop**, not derived internally — because there's no reliable backend signal for it (see the notifications-store note below), the _caller_ (a `features/notifications` query hook backed by `shared/stores/notifications.store.ts`) is responsible for deciding read/unread; this component only renders whatever it's told.
- **`NotificationIcon` maps free-text `category`/`type` strings to icons**, unlike `StockStatusBadge`/`OrderStatusBadge`'s opaque-enum handling — `category`/`type` are schema'd as plain strings, not a numeric enum, so mapping a few conventional values to nicer icons (falling back to a generic bell) doesn't risk misrepresenting a confirmed-but-unknown backend mapping the way guessing at `NotificationStatus`'s 1/2/3 would.

## The bell needs to inject into shared shell chrome without shared importing features

`NotificationBell` (the real, composed one) lives in `features/notifications`, but the topbar that mounts it is `shared/layout/Topbar.tsx`. Per [decisions/0013-shared-shell-callback-props.md](./0013-shared-shell-callback-props.md) — which explicitly anticipated this exact situation — `Topbar`/`AdminShell` gained an optional `notificationSlot?: ReactNode` prop, threaded through the same way `onLogout` already was. `app/(admin)/layout.tsx` (which is allowed to import both `shared/layout` and `features/notifications`) supplies the real bell. `shared/layout` never imports `features/notifications`.

## Why there's no reliable "unread" signal, and what was built instead

`UserNotificationSummaryResponse` (the list DTO) has no `readAt` field — only `GetUserNotificationResponse` (the single-detail DTO) does. `NotificationStatus` is an opaque, unpublished 3-value enum (see [backend/notification/README.md](../backend/notification/README.md)), so it cannot safely be interpreted as "unread"/"read" without guessing which number means what — the same "do not invent enum semantics" rule applied to `OrderStatus`/`WarehouseStatus` throughout this project. Instead, `shared/stores/notifications.store.ts` persists a `readIds` set of notification ids this browser has explicitly marked read through this UI (real user actions, not guessed backend state); "unread" is computed as "loaded and not in that set." Same browser/session-scoped caveat as every other local-tracking store in this app (Orders, Warehouses, Inventory) — a notification marked read from another client won't be reflected here. See [modules/notification-center.md](../modules/notification-center.md).

## Alternatives considered

- **Guess a `NotificationStatus` → unread/read mapping**: rejected outright — the exact category of invented-backend-behavior this project has consistently avoided.
- **Put these components in `shared/entity`**: rejected — `shared/entity` is admin-CRUD scaffolding (ADR 0014); a notification feed isn't a CRUD screen.
- **Skip the slot-prop and just re-add a real `NotificationBell` directly to `shared/layout`**: rejected — would require `shared/layout` to import `features/notifications` (query hooks, mutations, realtime), violating the one-way `app → features → shared` dependency rule for the same reason ADR 0013 rejected it for logout.

## Consequences

- Any future module needing a mini notification list/badge reaches for `shared/notifications` first.
- If the backend ever adds a real read/unread field or an unread-count endpoint, only `shared/stores/notifications.store.ts` and `features/notifications/api/notifications.queries.ts` need to change — none of `shared/notifications`'s components reference the workaround directly, they just render whatever `isRead`/`count` they're given.

## Related documents

[decisions/0013-shared-shell-callback-props.md](./0013-shared-shell-callback-props.md), [decisions/0015-shared-commerce-component-layer.md](./0015-shared-commerce-component-layer.md), [decisions/0016-shared-inventory-component-layer.md](./0016-shared-inventory-component-layer.md), [modules/notification-center.md](../modules/notification-center.md), [backend/notification/README.md](../backend/notification/README.md), [realtime/signalr-strategy.md](../realtime/signalr-strategy.md)
