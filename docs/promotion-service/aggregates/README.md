# Promotion Service — Aggregates

**Scope:** One doc per aggregate, written as each is implemented following step 12 ("Aggregate Boundaries") of [../entities/entity-implementation-strategy.md](../entities/entity-implementation-strategy.md).

- [campaign.md](campaign.md) — `Campaign` (Phase 2.1)
- [promotion.md](promotion.md) — `Promotion` (Phase 2.1)
- [coupon.md](coupon.md) — `Coupon` (Phase 2.2)
- [voucher.md](voucher.md) — `Voucher` (Phase 2.2)
- [loyalty.md](loyalty.md) — `LoyaltyProgram` (Phase 2.3)
- [reward.md](reward.md) — `RewardProgram` (Phase 2.3)
- [distribution.md](distribution.md) — `DistributionJob` (Phase 2.3)
- [recommendation.md](recommendation.md) — `RecommendationProgram` (Phase 2.4)
- [product-set.md](product-set.md) — `ProductSet` (Phase 2.4)
- [gift.md](gift.md) — `GiftProgram` (Phase 2.4)
- [approval.md](approval.md) — `ApprovalWorkflow` (Phase 2.4)
- [validation.md](validation.md) — Validation entities, no aggregate root (Phase 2.4)
- [audit.md](audit.md) — Audit entities, no aggregate root (Phase 2.4)

Every aggregate the roadmap named is now implemented, and the Phase 2.5 Domain Standardization Review has run — every doc above reflects the final, frozen shape: `EntityCode`/`Period` Value Objects consolidated across aggregates, `ProgramStatus` enum consolidated across Loyalty/Reward/Recommendation/Gift, and Translation support (`{Entity}Translation` + `Translate(...)`) added to every customer-facing aggregate. See [../planning/PROGRESS.md](../planning/PROGRESS.md) — **the Domain model is now frozen; Phase 3 (Persistence) is current.**

Translatable aggregates: Campaign, Promotion, Coupon, Voucher, LoyaltyProgram, RewardProgram, GiftProgram, RecommendationProgram, ProductSet, ProductBundle — see [../entities/translation-workflow.md](../entities/translation-workflow.md) for the frozen Translation implementation order future CQRS/API work follows.
