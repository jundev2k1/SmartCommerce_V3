# Domain — TODO

**Phase:** 2 (Domain Model). Done so far: 2.1 (Domain Foundation + Campaign/Promotion aggregates), 2.2 (Coupon/Voucher aggregates), 2.3 (Loyalty/Reward/Distribution aggregates) — see [../../../docs/promotion-service/aggregates/](../../../docs/promotion-service/aggregates/) for all seven. Next: 2.4 (Recommendation + Product Set + Gift).

Implement every remaining aggregate the architect's design specifies, following the fixed 12-step order in [../../../docs/promotion-service/entities/entity-implementation-strategy.md](../../../docs/promotion-service/entities/entity-implementation-strategy.md) (Entity → EntityData → Properties → Navigation Properties → Translation → Enums → Value Objects → Owned Types → Indexes & Unique Constraints → Relationships → Structural methods → Aggregate Boundaries & Future TODO).

Remaining Domain prompts, per the current roadmap:
- 2.4 — Recommendation + Product Set + Gift
- 2.5 — Approval + Validation + Audit (also where `CampaignApproval`/`CouponApproval`'s real workflow gets designed, deferred from 2.1/2.2)
- 2.6 — Domain Review + single Domain build (no build performed for any prompt before this one)

No EF Core, no MediatR, no repository types belong in this project — see [../../../docs/promotion-service/phases/phase-2-domain-model.md](../../../docs/promotion-service/phases/phase-2-domain-model.md).
