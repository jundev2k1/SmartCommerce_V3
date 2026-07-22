# Backend: Notification Service

**Purpose:** What the Notification Service exposes and how `src/services/notification` maps to it.

**Scope:** This service's contract only.

**Related documents:** [backend/README.md](../README.md), [backend/feature-mapping.md](../feature-mapping.md), [backend/coverage-report.md](../coverage-report.md), `src/services/notification/`, [realtime/signalr-strategy.md](../../realtime/signalr-strategy.md)

**When to read:** Building the Notifications module (Phase 10) — both the in-app notification center and, later, the reserved-but-unimplemented Campaign/Rule/Group/Channel admin screens.

**When to ignore:** Any work not touching notifications.

---

## Purpose

The full notification subsystem backing five distinct concepts, all present in this contract even though only "Notifications" (user-facing) is an implemented nav entry per [modules/overview.md](../../modules/overview.md) — the other four (Campaigns, Rules, Groups, Channels) are reserved-but-disabled nav placeholders, yet their backend endpoints already exist:

- **Channels** — delivery mechanisms (email, Telegram bot, etc.) and their runtime configuration/validation state.
- **Templates** — reusable, channel-scoped message templates.
- **Groups** — target audiences that campaigns broadcast to.
- **Rules** — "when event X happens, notify via targets Y" (event-triggered).
- **Campaigns** — broadcast (once or recurring) to a group's audience.
- **Dispatches** — the actual send attempts/results for a rule or campaign target.
- **User Notifications** — the per-user Notification Center entries (this is what Phase 10's bell icon reads).

## Base path

`/api/notification` (separate dev port, `localhost:5108`).

## Auth requirements

Not explicitly documented per-endpoint except `CreateUserNotification` ("Admin only — no automatic rule/campaign trigger is wired up yet").

## Endpoints (23)

| Area               | Operations                                                                                                                                                                                                                |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Campaigns          | `CreateNotificationCampaign` (starts Draft — no activate endpoint yet), `GetNotificationCampaign`, `ListNotificationCampaigns`                                                                                            |
| Channels           | `GetNotificationChannel`, `ListNotificationChannels` (unpaginated array), `UpdateNotificationChannelConfiguration` (resets validation), `EnableNotificationChannel` (requires Valid config), `DisableNotificationChannel` |
| Dispatches         | `GetNotificationDispatch`, `ListNotificationDispatches`                                                                                                                                                                   |
| Groups             | `CreateNotificationGroup`, `ListNotificationGroups`, `GetNotificationGroup`                                                                                                                                               |
| Rules              | `CreateNotificationRule`, `ListNotificationRules`, `GetNotificationRule`                                                                                                                                                  |
| Templates          | `CreateNotificationTemplate`, `ListNotificationTemplates`, `GetNotificationTemplate`                                                                                                                                      |
| User Notifications | `CreateUserNotification` (admin), `ListMyUserNotifications`, `GetUserNotification`, `MarkUserNotificationAsRead`                                                                                                          |

## Request/response DTOs

`NotificationCampaignTargetInput`/`Response` and `NotificationRuleTargetInput`/`Response` are near-identical shapes (channel/templateId/priority) reused across Campaign and Rule create/get — kept as separate types per their separate Swagger schemas (`types/campaign-target.ts`, `types/rule-target.ts`) rather than merged, since campaigns and rules are independent domain concepts that happen to share a shape today. See `src/services/notification/`.

## Pagination style

Page-based, `BackendPaginatedResult<T>` for Campaigns/Dispatches/Groups/Rules/Templates/User-Notifications lists. `ListNotificationChannels` is the one exception — a plain unpaginated array (`NotificationChannelSummaryResponseIReadOnlyListApiResponse`), since channel counts are small/fixed.

## Error responses

No prose error-response documentation at all for most endpoints in this file (unlike other services) beyond the general envelope — genuinely under-documented; see limitations.

## Known limitations / TODOs

- [ ] **12 enums, zero published mappings.** `AudienceType`, `CampaignExecutionType`, `CampaignStatus`, `ChannelValidationStatus`, `NotificationChannelStatus`, `NotificationChannelType`, `DispatchStatus`, `NotificationGroupStatus`, `NotificationPriority`, `NotificationRuleStatus`, `NotificationStatus`, `NotificationTemplateStatus` — all bare integers with no `x-enumNames` or prose mapping anywhere in the Swagger document. All kept opaque (`number`) in `types/enums.ts`. This is the single biggest documentation gap across all 7 services — worth escalating to the backend team before building any UI that needs to _display_ these as labels (status badges, priority icons, channel-type icons all need real mappings).
- [ ] **No campaign "Activate" endpoint.** `CreateNotificationCampaign`'s own description says campaigns "start in Draft - call Activate separately once execution is implemented," but no such endpoint exists in this contract yet.
- [ ] Almost no error-response prose at all for this service (contrast with Product/Inventory, which document 400/404/409 per endpoint) — assume standard REST semantics but don't rely on specific codes until confirmed.
- [ ] `audienceConfigJson` (Group) and `configJson` (Channel) are opaque JSON strings whose actual shape depends on `audienceType`/`channelType` — not modeled at all in the schema. The frontend can't validate these client-side without the backend publishing per-type sub-schemas.
