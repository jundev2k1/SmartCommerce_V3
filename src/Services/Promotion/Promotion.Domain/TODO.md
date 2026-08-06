# Domain — TODO

**Phase:** 2 (Domain Model). Done: 2.1 (Domain Foundation + Campaign/Promotion), 2.2 (Coupon/Voucher), 2.3 (Loyalty/Reward/Distribution), 2.4 (Recommendation/Product Set/Gift/Approval/Validation/Audit) — see [../../../docs/promotion-service/aggregates/](../../../docs/promotion-service/aggregates/) for all thirteen. Every aggregate the roadmap named is now implemented. Next: 2.5 (Domain Review + single Domain build) — the final Phase 2 prompt.

The 12-step order (Entity → EntityData → Properties → Navigation Properties → Translation → Enums → Value Objects → Owned Types → Indexes & Unique Constraints → Relationships → Structural methods → Aggregate Boundaries & Future TODO) from [../../../docs/promotion-service/entities/entity-implementation-strategy.md](../../../docs/promotion-service/entities/entity-implementation-strategy.md) was followed for every aggregate above.

Remaining Domain prompt:
- 2.5 — Domain Review + single Domain build. This is the first `dotnet build` for `Promotion.Domain` since Phase 1 — every entity/enum/Value Object across all 13 aggregate groups (94 entities, 24 enums, 15 PromotionService-local Value Objects + 1 Metadata Value Object) has been manually cross-checked signature-by-signature but never compiled.

Known open items for that review pass to confirm with the architect, not fix silently:
- `CampaignType`/`PromotionType` enum values are structural placeholders (Phase 2.1), not architect-specified.
- `RewardProgram.Code`/`DistributionJob.Code`/`GiftProgram.Code` are plain `string` (no Value Object), unlike every other aggregate's `Code` field.
- `LoyaltyProgram`/`RecommendationProgram` have no `TimeZone` property, unlike Campaign/Promotion/Coupon/Voucher.
- The Validation and Audit entity groups have no aggregate root, no `TenantId`/`ITenantEntity` — flagged as a literal reading of an omission in both briefs.
- `CampaignApproval`/`CouponApproval` remain unwired to the now-implemented `ApprovalWorkflow` structure (Phase 2.4) — "do not modify previous aggregates" was respected each time.

No EF Core, no MediatR, no repository types belong in this project — see [../../../docs/promotion-service/phases/phase-2-domain-model.md](../../../docs/promotion-service/phases/phase-2-domain-model.md).
