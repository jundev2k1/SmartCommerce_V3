# Domain — DONE (Phase 2 complete, frozen)

**Phase 2 (Domain Model) is complete as of Phase 2.5 (Domain Standardization Review).** `Promotion.Domain` builds clean — 103 entities, 21 enums, 4 PromotionService-local Value Objects + 1 Metadata Value Object, across 13 aggregate groups. See [../../../docs/promotion-service/aggregates/](../../../docs/promotion-service/aggregates/) for the full per-aggregate breakdown and [../../../docs/promotion-service/entities/translation-workflow.md](../../../docs/promotion-service/entities/translation-workflow.md) for the frozen Translation feature order.

**The Domain model is now frozen** — do not modify it in future phases unless a design defect is discovered. Phase 3 (Persistence) is current; no EF Core, no MediatR, no repository types belong in this project — see [../../../docs/promotion-service/phases/phase-3-persistence.md](../../../docs/promotion-service/phases/phase-3-persistence.md).

## What Phase 2.5 changed (standardization, not new business logic)

- Consolidated 7 duplicate Code Value Objects → `EntityCode`; 6 duplicate Period Value Objects → `Period`; 4 duplicate Program status enums → `ProgramStatus`.
- Renamed `CampaignLocalization` → `CampaignTranslation` for naming consistency.
- Added Translation support (`{Entity}Translation` entity + upsert `Translate(...)`) to Promotion, Coupon, Voucher, LoyaltyProgram, RewardProgram, GiftProgram, RecommendationProgram, ProductSet, and `ProductBundle` (10 aggregates translatable in total, counting Campaign).

## Known open items — flag for architect confirmation, do not fix silently

- `CampaignType`/`PromotionType` enum values are structural placeholders (Phase 2.1), not architect-specified.
- `RewardProgram.Code`/`DistributionJob.Code`/`GiftProgram.Code` are plain `string` (no Value Object), unlike every other aggregate's `Code` field.
- `LoyaltyProgram`/`RecommendationProgram` have no `TimeZone` property, unlike Campaign/Promotion/Coupon/Voucher.
- The Validation and Audit entity groups have no aggregate root, no `TenantId`/`ITenantEntity` — a literal reading of an omission in both briefs.
- `CampaignApproval`/`CouponApproval` remain unwired to the `ApprovalWorkflow` structure (Phase 2.4) — "do not modify previous aggregates" was respected each time this came up.
