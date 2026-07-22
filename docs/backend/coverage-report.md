# API Coverage Report

**Purpose:** Record of what Phase 1.5 processed from the 7 provided Swagger files — every endpoint's integration status, plus everything ambiguous, missing, or worth raising with the backend team.

**Scope:** A snapshot as of the Swagger files provided in Phase 1.5. Re-run this exercise (and update this report) whenever the backend provides updated Swagger.

**Related documents:** [backend/README.md](./README.md), [backend/feature-mapping.md](./feature-mapping.md), all per-service docs

**When to read:** Before assuming an endpoint exists, or when deciding whether a frontend module can be built against the current contract.

**When to ignore:** Once you've confirmed the specific endpoint(s) you need are covered — no need to read the whole report for a single-endpoint question.

---

## Endpoint checklist (69/69 integrated)

### Audit (2/2)

- ✓ GetAuditLog
- ✓ ListAuditLogs

### Auth (4/4)

- ✓ Login
- ✓ Logout
- ✓ RefreshToken
- ✓ Register

### User (4/4)

- ✓ CreateUser
- ✓ GetUser
- ✓ GetUserDetail
- ✓ UpdateUser

### Order (3/3)

- ✓ CreateOrder
- ✓ GetOrder
- ✓ CancelOrder

### Inventory (8/8)

- ✓ GetInventory
- ✓ GetInventoryHistory
- ✓ GetProductStock
- ✓ StockIn
- ✓ StockOut
- ✓ AdjustStock
- ✓ CreateWarehouse
- ✓ GetWarehouse

### Product (25/25)

- ✓ CreateProduct, GetProduct, UpdateProduct, DeleteProduct, SearchProducts
- ✓ AddVariation, UpdateVariation, DeleteVariation, ReorderVariations, SetDefaultVariation
- ✓ CreateProductCategory, GetProductCategory, UpdateProductCategory, DeleteProductCategory, ListProductCategories, AssignProductCategory, RemoveProductCategory
- ✓ CreateProductTag, GetProductTag, UpdateProductTag, DeleteProductTag, ListProductTags, AssignProductTag, RemoveProductTag
- ✓ RebuildProductSearchIndex

### Notification (23/23)

- ✓ CreateNotificationCampaign, GetNotificationCampaign, ListNotificationCampaigns
- ✓ GetNotificationChannel, ListNotificationChannels, UpdateNotificationChannelConfiguration, EnableNotificationChannel, DisableNotificationChannel
- ✓ GetNotificationDispatch, ListNotificationDispatches
- ✓ CreateNotificationGroup, ListNotificationGroups, GetNotificationGroup
- ✓ CreateNotificationRule, ListNotificationRules, GetNotificationRule
- ✓ CreateNotificationTemplate, ListNotificationTemplates, GetNotificationTemplate
- ✓ CreateUserNotification, ListMyUserNotifications, GetUserNotification, MarkUserNotificationAsRead

## Unsupported / missing endpoints (needed by a planned frontend module, not present in the contract)

- **User list/search/delete** — no `GET /profiles` (list) and no `DELETE /profiles/{userId}` exist; only Create, Get-by-id, Get-current-detail, and Update. Confirmed blocking in Phase 2: the Users page can only ever show one real row (the current user) and Delete is permanently disabled until these land. _(This gap existed since Phase 1.5 but wasn't listed here until Phase 2 actually hit it in practice — see [modules/user-management.md](../modules/user-management.md).)_
- **Order list/search** — no `GET /orders` exists; only `GetOrder(orderId)`. Blocks a real Order History table (Phase 8).
- **Warehouse list/search** — no `GET /warehouses` exists; only `GetWarehouse(warehouseId)`. Blocks a real Warehouses table (Phase 9).
- **Inventory list/search** — no `GET /inventories` exists; only `GetInventory(inventoryId)`. An Inventory table (Phase 9) would need to be driven by product/variation instead, or this endpoint needs to be added.
- **Notification Campaign "Activate"** — `CreateNotificationCampaign`'s own prose says campaigns start Draft "call Activate separately once execution is implemented," but no such endpoint exists yet.
- **Category/tag usage counts** — no endpoint reports how many products reference a given category/tag before attempting a delete.

## Deprecated endpoints

None found — every endpoint in all 7 files appears current/active.

## Duplicate endpoints

None found across services (each service owns a clearly distinct resource space; no path collisions once namespaced by `servers.url`).

## Missing schema definitions

- None of the 7 Swagger files model a **non-2xx response schema** for any endpoint — every `responses` block only documents `200`/`201`. Error behavior (404/400/409/500) is described only in free-text prose per endpoint. The frontend's `ApiError` normalization (see [services/api-layer.md](../services/api-layer.md)) is designed generically enough to not depend on a specific error schema, but nothing here confirms the actual error response body shape at runtime — worth validating once a real backend environment is reachable.
- Two endpoints (`user/GetUserDetail`, prose only) reference behavior/parameters not reflected in their actual `parameters` array — see the User service doc's "Known limitations."

## Ambiguous DTOs / contracts

- **Auth's `Bearer` security scheme vs. cookie-based prose.** The Auth Swagger declares a `Bearer` scheme and a top-level security requirement, but every endpoint's description explicitly states cookie-based auth with no tokens in the response body. Frontend follows the prose + ADR 0005 (cookies); the Bearer scheme is treated as backend-framework boilerplate. **Recommend confirming with the backend team which is authoritative.**
- **12 numeric enums with zero published name mappings** across Notification (`AudienceType`, `CampaignExecutionType`, `CampaignStatus`, `ChannelValidationStatus`, `NotificationChannelStatus`, `NotificationChannelType`, `DispatchStatus`, `NotificationGroupStatus`, `NotificationPriority`, `NotificationRuleStatus`, `NotificationStatus`, `NotificationTemplateStatus`), plus `AuditAction` (Audit), `OrderStatus` (Order), `WarehouseStatus`/`InventoryTransactionType` (Inventory), `ProductCategoryStatus` (Product) — 17 enums total kept as opaque `number` rather than guessed unions. **The single largest actionable item from this integration**: get these mappings from the backend before building any status badge, priority icon, or filter dropdown that needs to _label_ these values.
- **Product variation/search `status` is a free-text string**, not an enum $ref, despite prose naming exactly 3 values ("Active"/"Inactive"/"Discontinued") — the schema doesn't constrain it, so it's typed as `string`, not a closed union, to avoid rejecting a legitimate backend value the union didn't anticipate.
- **Request DTOs are schema'd as universally nullable** (typical loose C# Swagger generation) even where prose clearly states a field is required — frontend request types follow the prose (required fields non-optional), not the loose schema. If the backend ever tightens the schema to match, no frontend change is needed; if a "required" field turns out to genuinely be optional in practice, the frontend types will need loosening.

## Missing examples

None of the 7 Swagger files include `example`/`examples` values on any schema or parameter — every DTO shape here was derived purely from the `type`/`properties`/`nullable` declarations and endpoint prose, with no sample payloads to cross-check against.
