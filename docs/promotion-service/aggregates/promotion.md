# Promotion Aggregate

**Scope:** Domain-layer facts for the `Promotion` aggregate, implemented Phase 2.1. Structural only — no business rule (rule evaluation, discount calculation, stacking resolution) is implemented yet.

## Aggregate boundary

`Promotion` (`Promotion.Domain/Entities/Promotions/Promotion.cs`) is the aggregate root — `AggregateRoot<Guid>`, `IAuditable`, `ITenantEntity`. It optionally belongs to a `Campaign` (`CampaignId`, FK only, no navigation back). It owns, via navigation + internal construction (Rule 3): `PromotionVersion`, `PromotionRule`, `PromotionTarget`, `PromotionBenefit`, `PromotionConstraint`, `PromotionUsageLimit` (collections), and `PromotionExecutionPolicy`/`PromotionStackingPolicy` (strict 1:1, shared-PK detail tables, Rule 5) plus `PromotionMetadata` (owned Value Object, not an entity — see below).

`PromotionRuleGroup`, `PromotionCondition`, `PromotionPriority`, `PromotionExclusion` are **not** part of this aggregate's direct navigation graph, matching the phase brief's own Navigation list (which omitted all four):
- `PromotionRuleGroup` — related by `PromotionId`; `PromotionRule.RuleGroupId` points to it.
- `PromotionCondition` — related by `RuleId` (one level deeper than Promotion — belongs to a `PromotionRule`, not directly to Promotion).
- `PromotionPriority` — related by `PromotionId`; a typed priority classification distinct from the plain `Promotion.Priority` ordering int.
- `PromotionExclusion` — pure mapping row between two `Promotion` aggregate roots (`PromotionId`/`ExcludedPromotionId`), same shape as `Order.Domain.Entities.Orders.OrderTag` (Rule 4, no surrogate `Id`).

## PromotionMetadata is a Value Object, not an entity

Despite being listed under "Entities," `PromotionMetadata` is implemented as a `MetadataBase`-derived Value Object in `Promotion.Domain/Metadata/PromotionMetadata.cs` — matching the platform's existing `ProductMetadata`/`UserMetadata`/`DiscountMetadata`/`TenantMetadata` convention (a flexible, dictionary-backed extension bag with typed accessor properties), not a separate table with its own `Id`. `Promotion.Metadata` is a nullable owned reference, set via `Promotion.SetMetadata(...)`.

## Entities

| Entity | Shape | Notes |
|---|---|---|
| `Promotion` | `AggregateRoot<Guid>` | CampaignId/Code/Name/Description/Status/Type/Priority/Version/StartTime/EndTime/Currency/TimeZone/IsEnabled |
| `PromotionVersion` | `BaseEntity<Guid>` | Append-only marker bumped by `Promotion.IncrementVersion`/`AddVersion` — no real snapshot payload yet |
| `PromotionRule` | `BaseEntity<Guid>` | Optional `RuleGroupId`, no condition evaluation |
| `PromotionRuleGroup` | `BaseEntity<Guid>` | AND/OR combinator label, no evaluation |
| `PromotionCondition` | `BaseEntity<Guid>` | Structural (Field, Operator, Value) triple, belongs to a `PromotionRule` |
| `PromotionTarget` | `BaseEntity<Guid>` | Structural (TargetType, TargetKey) pair |
| `PromotionBenefit` | `BaseEntity<Guid>` | Structural (BenefitType, Value) pair, no discount math |
| `PromotionConstraint` | `BaseEntity<Guid>` | Structural (ConstraintType, Value) pair |
| `PromotionPriority` | `BaseEntity<Guid>` | `PromotionPriorityType` + `PromotionPriorityValue`, no conflict resolution |
| `PromotionExecutionPolicy` | `BaseEntity` (shared PK) | `PromotionExecutionMode` + `MaxExecutionsPerOrder` |
| `PromotionStackingPolicy` | `BaseEntity` (shared PK) | `PromotionStackingMode` |
| `PromotionUsageLimit` | `BaseEntity<Guid>` | Structural (Scope, MaxUsage) pair, no counting/enforcement |
| `PromotionExclusion` | `BaseEntity` (pure mapping) | `PromotionId` ↔ `ExcludedPromotionId` |
| `PromotionTranslation` | `BaseEntity<Guid>` | Added Phase 2.5: `Id = Promotion.Id`, composite `(Id, LanguageCode)`. Exposed via `Promotion.Translate(languageCode, name, description)` — upsert, per [../entities/translation-workflow.md](../entities/translation-workflow.md) |

## Enums

- `PromotionStatus` — Draft/Active/Paused/Expired/Cancelled.
- `PromotionType` — structural placeholder taxonomy (PercentageOff/FixedAmountOff/BuyXGetY/FreeShipping/Bundle/Custom); not architect-specified, confirm before treating as final. Deliberately avoids "Coupon" as a value since Coupon is its own future aggregate (Phase 2.2, next up).
- `PromotionPriorityType` — Low/Normal/High/Critical.
- `PromotionExecutionMode` — Automatic/Manual/Scheduled.
- `PromotionStackingMode` — NotStackable/StackWithAny/StackWithSameType/StackWithSpecific.

## Value Objects

- `EntityCode` (shared, consolidated Phase 2.5) — used by `Promotion.Code`.
- `Period` (shared, consolidated Phase 2.5, reserved) — `Promotion` itself keeps `StartTime`/`EndTime`/`TimeZone` as plain scalars per the phase brief's literal Properties list (same reconciliation as Campaign).
- `PromotionPriorityValue` — validated `int` 0-100 (mirrors `BuildingBlock.Domain.ValueObjects.Percentage`'s shape), used only by `PromotionPriority`. Not merged with anything — unique shape.

## Indexes (design only — written in Phase 3)

- `(Code)` unique
- `(CampaignId)`
- `(Status)`
- `(Status, StartTime)`
- `(Type, Status)`
- `(Priority)`

## Reconciliation notes

Same as Campaign's — no `PromotionData`/`UpdateData`/`PromotionTranslationData` wrapper created; `Promotion.Create` takes flat scalar parameters, every child is added via a separate method, every update/translate method takes flat parameters (`ProductBrand.UpdateDetails` precedent).
