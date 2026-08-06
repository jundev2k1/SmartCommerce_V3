# Task 6: Promotion Service — Phase 2.1 Campaign + Promotion Aggregates

**Status:** Done (Domain layer only, structural, per the prompt's own scope)
**Category:** Domain-layer entity implementation, no business logic, no build

## What was done

Implemented the `Campaign` and `Promotion` aggregates in full — every entity, translation entity, enum, Value Object, and relationship the prompt listed — following the 12-step order frozen in Task 5's strategy doc. No Persistence, Infrastructure, CQRS, API, or Search touched; no business rule (eligibility, discount calculation, stacking resolution, approval workflow) implemented; no build run, per the prompt's explicit policy.

**Campaign aggregate** (`Promotion.Domain/Entities/Campaigns/`) — 9 entities: `Campaign` (root, `AggregateRoot<Guid>`/`IAuditable`/`ITenantEntity`), `CampaignSchedule`, `CampaignAudience`, `CampaignChannel`, `CampaignTag`, `CampaignAttachment`, `CampaignLocalization` (Translation pattern, Rule 5), `CampaignBudget`, `CampaignApproval` (structural placeholder, real workflow deferred to the Approval/Validation/Audit prompt). 2 enums (`CampaignStatus`, `CampaignType`), 2 Value Objects (`CampaignCode`, `CampaignPeriod`). Full detail: [docs/promotion-service/aggregates/campaign.md](../../promotion-service/aggregates/campaign.md).

**Promotion aggregate** (`Promotion.Domain/Entities/Promotions/`) — 13 entities: `Promotion` (root), `PromotionVersion`, `PromotionRule`, `PromotionRuleGroup`, `PromotionCondition`, `PromotionTarget`, `PromotionBenefit`, `PromotionConstraint`, `PromotionPriority`, `PromotionExecutionPolicy`/`PromotionStackingPolicy` (shared-PK 1:1, Rule 5), `PromotionUsageLimit`, `PromotionExclusion` (pure mapping, Rule 4). Plus `PromotionMetadata`, reclassified from the prompt's "Entities" list into a `MetadataBase`-derived Value Object (`Promotion.Domain/Metadata/`) — matching the platform's `ProductMetadata`/`DiscountMetadata` convention, not a new table. 5 enums (`PromotionStatus`, `PromotionType`, `PromotionPriorityType`, `PromotionExecutionMode`, `PromotionStackingMode`), 3 Value Objects (`PromotionCode`, `PromotionPeriod`, `PromotionPriorityValue`). Full detail: [docs/promotion-service/aggregates/promotion.md](../../promotion-service/aggregates/promotion.md).

**Reconciliations made against the prompt's literal wording** (consistent with Task 5's precedent, all recorded in the two aggregate docs):
- **No `EntityData`/`UpdateData` wrapper types created.** No child collection is structurally mandatory at Campaign/Promotion creation (no stated "≥1 X" business rule), so both `Create` factories take flat scalar parameters (`docs/conventions/domain-coding-conventions.md` Rule 2) and children are added via separate `Add{Child}` methods; every update method (`UpdateDetails`, `Reschedule`, ...) is likewise flat-parameter, matching `ProductBrand.UpdateDetails`.
- **`CampaignBudget`/`CampaignApproval` and `PromotionRuleGroup`/`PromotionCondition`/`PromotionPriority`/`PromotionExclusion` are not navigation collections on their aggregate root** — the prompt's own Navigation lists omitted all six, so each is related by FK id only (`CampaignId`/`PromotionId`/`RuleId`), with `internal` `Create` methods that exist but currently have no caller (matches Payment Service's precedent of EF-config-ready-but-uncalled entities for aggregates outside the current CQRS/construction scope).
- **`PromotionMetadata` implemented as a Value Object, not an entity** (see above) — the platform has an established `Metadata/` Value Object convention this fits exactly; creating a new table for it would contradict that convention.
- **Enum values (`CampaignType`, `PromotionType`) are structural placeholders**, not architect-specified — populated with a conventional promotion-engine taxonomy and flagged with a `// TODO` comment plus a doc callout, since an enum requires *some* members to compile and none were given.

## Objective

Give Phase 2.2+ (Coupon + Voucher, and every aggregate after it) two complete, convention-correct reference aggregates to pattern-match against, with zero business logic to accidentally copy forward.

## Current state (grounded findings)

- `Promotion.Domain` now has 22 entity files, 1 Metadata Value Object, 7 enums, 5 Value Objects — verified for internal consistency (every `Create`/`internal` call site in `Campaign.cs`/`Promotion.cs` cross-checked argument-by-argument against the target entity's actual factory signature) but **not build-verified** — the prompt's build policy explicitly forbids building before Phase 2.6 (Domain Review + single Domain build).
- Fixed one real defect found during this pass: `Campaign.Promotions`'s navigation type (`Promotion`) collides with the project's own root namespace (`NovaCore.Promotion.Domain`) exactly the way `Payment`/`Order`/`Product`/`User` already document for their own root-namespaced entity — added the same `global using PromotionEntity = NovaCore.Promotion.Domain.Entities.Promotions.Promotion;` alias pattern to `GlobalUsings.cs` before it could become a latent compile error at Phase 2.6.

## Scope

**Built this prompt:** the two aggregates in full, per the entity/enum/VO lists above.

**Explicitly not built:** any Persistence/Infrastructure/CQRS/API/Search code; any business rule; any build.

## Dependencies

Phase 2.1 Domain Foundation (Task 5). Phase 2.2 (Coupon + Voucher) depends on this pair existing as the pattern reference.

## Estimated complexity

Large (23 new Domain types across 2 aggregates in one session, no build to catch mistakes — relied on manual signature cross-checking instead).

## Risks

- Not build-verified — if a mistake survived the manual review, it won't surface until Phase 2.6's single Domain build. Flagging explicitly per the task convention rather than silently assuming correctness.
- `CampaignType`/`PromotionType` enum values are genuinely invented (structural placeholders) — must be reconciled against the real architect design before this service leaves the planning/foundation stage, not just left as-is into Persistence.
