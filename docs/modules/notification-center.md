# Notification Center (In-App + Realtime)

**Purpose:** The single cross-cutting place in this app responsible for displaying user notifications — the topbar bell, its dropdown feed, and a detail dialog — fed by both normal fetches and realtime SignalR events. Business modules never implement their own notification UI; they'd reach for `shared/notifications`/`features/notifications` instead (none do yet, since no other module currently needs one).

**Scope:** `features/notifications`, the `shared/notifications` presentational layer, and the `notificationSlot` addition to `shared/layout`'s `AdminShell`/`Topbar`.

**Related documents:** [modules/overview.md](./overview.md), [backend/notification/README.md](../backend/notification/README.md), [realtime/signalr-strategy.md](../realtime/signalr-strategy.md), [decisions/0007-signalr-strategy.md](../decisions/0007-signalr-strategy.md), [decisions/0010-pagination-and-cursor-strategy.md](../decisions/0010-pagination-and-cursor-strategy.md), [decisions/0013-shared-shell-callback-props.md](../decisions/0013-shared-shell-callback-props.md), [decisions/0017-shared-notifications-component-layer.md](../decisions/0017-shared-notifications-component-layer.md).

**When to read:** Touching the notification bell/dropdown/detail dialog, cursor-list pagination, or wondering why unread counts are approximate.

**When to ignore:** The four reserved Notification-admin nav placeholders (Campaigns/Rules/Groups/Channels) — those remain disabled/unimplemented on purpose, see "Reserved admin placeholders" below.

---

## Feature folder(s)

`src/features/notifications/`. Presentational pieces in `src/shared/notifications/`. The generic cursor-pagination wrapper lives in `src/shared/hooks/useCursorList.ts` (feature-agnostic — any future feed reuses it, not just notifications).

## Where the bell lives, and why

`NotificationBell` (the real, data-owning component) is built in `features/notifications`, but it's mounted inside `shared/layout`'s `Topbar` — which can't import `features/*` (one-way dependency rule). Per [decisions/0013](../decisions/0013-shared-shell-callback-props.md) (which explicitly anticipated this), `AdminShell`/`Topbar` gained an optional `notificationSlot?: ReactNode` prop; `app/(admin)/layout.tsx` supplies `<NotificationBell />` from `features/notifications`'s barrel. `shared/layout`'s own placeholder `NotificationBell.tsx` (Phase 1) was deleted — the real one fully replaces it.

## One real contract gap that shaped every design decision here

- **No unread signal exists on the list shape, and no unread-count endpoint exists at all.** `UserNotificationSummaryResponse` (the list DTO) has no `readAt` field — only `GetUserNotificationResponse` (single-detail) does — and `NotificationStatus` is one of the 12 opaque, unpublished enums flagged in [backend/coverage-report.md](../backend/coverage-report.md). Guessing which of its 3 values means "unread" would be exactly the invented-backend-behavior this project consistently avoids. Instead, `shared/stores/notifications.store.ts` persists `readIds` — notification ids this browser has explicitly marked read through this UI (via the real `MarkUserNotificationAsRead` endpoint) — and "unread" is computed client-side as "loaded and not in that set." **This means the unread badge/count reflects "unread among notifications loaded into this browser," not a true historical count from the backend** — same browser-scoped caveat as Orders/Warehouses/Inventory's local-tracking stores, applied here for a different underlying reason (no signal at all, rather than no list endpoint at all).

## Notification Bell

`features/notifications/components/NotificationBell.tsx`. Calls `useNotificationsListQuery()` on mount — this fetches only page 1 (15 items), which is the "lightweight, don't preload history" bound the brief asked for. Computes the unread count from that page against `readIds`. Shows a small connection-status dot (green when the shared SignalR hub reports `'connected'`) as the brief's optional connection indicator. The popover trigger is a real button (`IconButton`), so it's keyboard-focusable/activatable for free via native semantics + Radix's `Popover` underneath — no custom keyboard handling was needed.

## Notification Dropdown

`features/notifications/components/NotificationDropdown.tsx` renders `shared/notifications`'s `NotificationList` with the **same** query result `NotificationBell` already fetched (passed as a prop, same query key) — opening the dropdown causes no extra network request. `NotificationList` groups by `category` (a real free-text field on the DTO) and loads more automatically via a real `IntersectionObserver` on a sentinel row — "only load older notifications when users explicitly scroll," not a fixed timer or full preload.

**"Mark all as read"** has no backend bulk endpoint — it's composed client-side as parallel calls to the real per-item `MarkUserNotificationAsRead` for whatever's currently loaded-and-unread. It cannot mark anything not yet fetched into the client; this is disclosed, not hidden.

## Notification Detail

`features/notifications/components/NotificationDetailDialog.tsx`, backed by the real `GetUserNotification`. Opening it marks the notification read (same convention as reading an email) in addition to the explicit per-item "mark as read" affordance in the list. Shows title/body/created-time/category/type/priority/expiry — all real fields on `GetUserNotificationResponse`. **"Related entity" and "Navigation action" are limited to a plain `campaignId` value with no link** — there's no generic entity-reference field on this DTO (unlike Audit's `rootEntityId`/`rootEntityType`), and no Campaign detail page exists yet (reserved nav placeholder only, this phase). Nothing was invented to fill this gap.

## Realtime

Reuses the exact infrastructure Phase 6 (Sales) built — `shared/lib/realtime/useHubEvent.ts` and the connection lifecycle already wired to auth session. `features/notifications/hooks/useNotificationRealtimeUpdates.ts` subscribes to a placeholder event, `'NotificationCreated'` (no event catalog is published by any of the 7 services — see [realtime/signalr-strategy.md](../realtime/signalr-strategy.md)). On receipt: prepends the new item directly into the cached first page via `setQueryData` (an already-open dropdown shows it immediately, no refetch) and increments the unread counter — never a full-list invalidation.

## Reserved admin placeholders

Campaign/Rule/Group/Channel nav entries were already added in Phase 1 with `implemented: false` and already render disabled/"coming soon" via `Sidebar.tsx` — nothing needed to change here; this phase only confirms they're still exactly that, per the brief's explicit "create placeholders only, do NOT implement business logic."

## API / State

`features/notifications/api/notifications.queries.ts` — `useNotificationsListQuery` (the shared cursor-list instance), `useNotificationQuery`, `useMarkAsReadMutation`. Server state stays entirely in TanStack Query; Zustand involvement is `notifications.store.ts` (unread count + `readIds`, both genuinely cross-component UI-ish state) and the pre-existing `signalr.store.ts` (connection status) — no new UI-only store (dropdown open/closed) was added to Zustand, since that's scoped to one component (`NotificationBell`'s own `useState`) and `docs/state/zustand-strategy.md` already says single-component state belongs in local state, not a store — a deliberate, documented deviation from the brief's literal wording favoring this project's own established rule.

## The `/notifications` nav route is a real full-history page

Originally left as `PlaceholderModulePage` (Phase 1 reserved the nav entry with `implemented: true`, but that phase's brief only asked for the Bell/Dropdown/Detail experience). Built later (see `docs/tasks/2026-07-22/Task7_inventory-warehouse-notification-integration-gaps.md`) as `NotificationsHistoryPage` (`features/notifications/components/`), reusing `shared/notifications`'s `NotificationList`/`NotificationActions` unmodified — same `useNotificationsListQuery` cache entry as the bell/dropdown, an `EntityHeader` + "mark all read", and `NotificationList`'s new `className` override prop (`max-h-none overflow-visible`) so the list scrolls with the page instead of being capped to dropdown height. No new realtime subscription — `NotificationBell`'s `useNotificationRealtimeUpdates()` already runs globally in the admin layout.

## Audit

Not applicable — notifications aren't an auditable admin-CRUD entity in this contract; no `AuditTrailButton` is used here.

## Dependencies on other modules

None. This is the first genuinely cross-cutting module — every other feature could depend on it (to publish/react to notifications) without it depending back on any of them, per the brief's "business modules should only publish or react to notifications."

## Open questions / backend dependencies

- If the backend publishes `NotificationStatus`'s value mapping, or adds an unread-count endpoint, `shared/stores/notifications.store.ts` and `notifications.queries.ts` are the only places to revisit — no shared/notifications component references the workaround directly.
- If a real SignalR event catalog is published, only `useNotificationRealtimeUpdates.ts`'s event name/payload needs to change.
