# Task 2: Promotion Service — Phase 2.4 Recommendation + Product Set + Gift + Approval + Validation + Audit

**Status:** Done (Domain layer only, structural, per the prompt's own scope)
**Category:** Domain-layer entity implementation, no business logic, no build

## What was done

Implemented six entity groups in one prompt — the largest single Domain-implementation task in this roadmap (32 new entities). No modification to any previously implemented aggregate.

**Recommendation aggregate** (`Promotion.Domain/Entities/Recommendations/`) — 5 entities: `RecommendationProgram` (root, owns `RecommendationRule`/`RecommendationProduct`), `RecommendationScore` (public `Create`, not even `ProgramId`-scoped — a standalone `ProductId`-keyed signal), `RecommendationHistory` (public `Create`). 2 enums, 2 Value Objects (`RecommendationPeriod` has no `TimeZone`, matching `RecommendationProgram` having none). Full detail: [docs/promotion-service/aggregates/recommendation.md](../../promotion-service/aggregates/recommendation.md).

**Product Set aggregate** (`Promotion.Domain/Entities/ProductSets/`) — 6 entities: `ProductSet` (root, owns `ProductSetItem`/`ProductBundle`), `BundlePrice`/`BundleRule`/`BundleGift` (public `Create`, related to `ProductBundle` by `BundleId` only — no Navigation section was given for `ProductBundle` itself). 2 enums, 1 Value Object (`ProductSetCode`). **First reuse of the shared `Quantity` Value Object** (`BuildingBlock.Domain.ValueObjects.Quantity`, the same one `Order.Domain`'s `OrderItem` uses) for `ProductSetItem.Quantity`/`BundleGift.Quantity`. Full detail: [docs/promotion-service/aggregates/product-set.md](../../promotion-service/aggregates/product-set.md).

**Gift aggregate** (`Promotion.Domain/Entities/Gifts/`) — 6 entities: `GiftProgram` (root, owns `GiftItem`), `GiftInventory`/`GiftReservation`/`GiftClaim`/`GiftUsage` (public `Create`, no Navigation section given). 1 enum, no Value Objects (brief gave none, `Code` stays plain string — same pattern as Reward/Distribution in Phase 2.3). `Quantity` reused again for every stock/reservation count field. Full detail: [docs/promotion-service/aggregates/gift.md](../../promotion-service/aggregates/gift.md).

**Approval aggregate** (`Promotion.Domain/Entities/Approvals/`) — 6 entities: `ApprovalWorkflow` (root, owns `ApprovalStep`), `ApprovalAssignment`/`ApprovalDecision`/`ApprovalComment`/`ApprovalHistory` (public `Create`, no Navigation section given). 1 enum, no Value Objects (`ApprovalWorkflow` has no `Code` field at all, unlike every other root this roadmap). This is the general-purpose workflow structure `CampaignApproval` (Phase 2.1) and `CouponApproval` (Phase 2.2) were both documented as deferring to — **not wired together** in this pass, since connecting them would mean modifying two previously implemented aggregates, explicitly out of scope. Full detail: [docs/promotion-service/aggregates/approval.md](../../promotion-service/aggregates/approval.md).

**Validation entities** (`Promotion.Domain/Entities/Validations/`) — 5 entities, **no aggregate root**: unlike every prior group, the brief gave no Properties/Navigation/`TenantId` for any of the 5 (`PromotionValidationPolicy`, `PromotionValidationResult`, `PromotionSimulation`, `PromotionSimulationScenario`, `PromotionSimulationResult`). Rather than inventing a root shape, all 5 stayed plain `BaseEntity<Guid>` with `IAuditable` only, no `ITenantEntity` — flagged explicitly as a literal reading of an omission, not an oversight on this task's part. They form two independent FK chains (Policy→Result, Simulation→Scenario→Result), not one aggregate. Full detail: [docs/promotion-service/aggregates/validation.md](../../promotion-service/aggregates/validation.md).

**Audit entities** (`Promotion.Domain/Entities/Audits/`) — 4 entities, same "no root" treatment as Validation: `PromotionAudit`, `RuleAudit`, `ApprovalAudit`, `ExecutionAudit` — all shaped exactly like every `*History` entity already implemented across this roadmap, just platform-level (not owned by one specific aggregate). `ApprovalAudit` is a distinct entity from `ApprovalHistory` (Entities/Approvals/) — both were implemented as separately named/requested, not treated as a duplicate. Full detail: [docs/promotion-service/aggregates/audit.md](../../promotion-service/aggregates/audit.md).

## Objective

Complete every remaining aggregate the roadmap named, so Phase 2.5 (Domain Review + the first `Promotion.Domain` build since Phase 1) has a fully populated Domain layer to compile and review.

## Current state (grounded findings)

- `Promotion.Domain` now has 62 (prior) + 32 (this phase) = 94 entity files, 1 Metadata VO, 18 (prior) + 6 (this phase) = 24 enums, 12 (prior) + 3 (this phase) = 15 PromotionService-local Value Objects. Every `Create`/`internal` call site cross-checked argument-by-argument against its target signature — not build-verified (Phase 2.5's job).
- No `GlobalUsings.cs` changes were needed this phase — none of the 32 new types collide with an existing type or the project's root namespace segment (unlike `Promotion`/`CouponCode` in earlier phases).

## Scope

**Built this prompt:** the six entity groups in full, per the lists above.

**Explicitly not built:** any Persistence/Infrastructure/CQRS/API/Search code; any business rule; any build; no modification to any previously implemented aggregate; no wiring between `ApprovalWorkflow` and `CampaignApproval`/`CouponApproval`.

## Dependencies

Phase 2.3 (Task 1, `2026-08-07`). Phase 2.5 (Domain Review + single Domain build) depends on every aggregate in this roadmap being complete, which this task finishes.

## Estimated complexity

Large (41 new Domain types across 6 entity groups in one prompt — the largest single implementation task this roadmap).

## Risks

- Not build-verified — same caveat as every prior Domain-implementation task, but now the stakes are highest: Phase 2.5 is the first compile of the entire 94-entity Domain layer, and any latent mistake across 5 prompts' worth of hand-written code surfaces there, not incrementally.
- Validation/Audit having no aggregate root, no `TenantId` is the most structurally unusual decision made across this whole roadmap — worth the architect's explicit confirmation before Phase 3 (Persistence) has to decide how to map them (which table owns them, whether they need tenant-scoped query filtering after all).
- `ApprovalWorkflow` existing but unwired to `CampaignApproval`/`CouponApproval` means the "Approval + Validation + Audit" theme originally promised by the roadmap is only partially delivered — the structure exists, the cross-aggregate wiring does not (by design, given the "don't modify previous aggregates" constraint).
