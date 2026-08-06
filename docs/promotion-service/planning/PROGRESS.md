# Promotion Service — Progress

**Scope:** Living status tracker for the Promotion Service implementation roadmap. This is the permanent progress-tracking convention for the whole project — read at the start of every phase/prompt, updated only at the end (never mid-work). Never recreate the roadmap; never restart the plan; always continue from what's recorded here.

## Current Project Status

**Completed Phases**
- ✔ Phase 0 — Planning Freeze
- ✔ Phase 1 — Bootstrap Service

**Current Phase**
- ▶ Phase 2 — Domain Model

**Current Prompt**
- ✔ 2.1 — Domain Foundation + Campaign/Promotion aggregates (done)
- ✔ 2.2 — Coupon + Voucher aggregates (done)
- ✔ 2.3 — Loyalty + Reward + Distribution aggregates (done)
- ✔ 2.4 — Recommendation + Product Set + Gift + Approval + Validation + Audit (done — renumbered by the issuing prompt to bundle all six into one sub-prompt, superseding the earlier 2.4/2.5 split)
- ▶ 2.5 — Domain Review + single Domain build (next, not yet started — final Phase 2 prompt)

**Overall Progress**
2 / 7 Completed

**Status**
In Progress

**Remaining Phases**
- □ Phase 3 — Persistence
- □ Phase 4 — Search Integration
- □ Phase 5 — CQRS Skeleton
- □ Phase 6 — Infrastructure Integration
- □ Phase 7 — Migration Preparation

## Progress Management Policy

- At the start of every phase/prompt: read this document. Never ask for progress clarification — always continue from what's recorded here.
- At the end of every phase: mark the completed phase, move the current-phase pointer, update the completed-phase count, update the status. Never recreate the roadmap or restart the plan.
- Within a phase, sub-prompts (e.g. Phase 2.1, 2.2, ...) are tracked under **Current Prompt** without moving the phase pointer or the Overall Progress count — the phase only counts as complete once every aggregate/prompt inside it is done.
- Sub-prompt numbering is authoritative from the most recently issued prompt — if a later prompt renumbers a sub-prompt, that renumbering wins and this file is updated to match (this happened once already: Domain Foundation + Campaign/Promotion were both folded into "2.1").
- **Commit granularity (added 2026-08-06):** every aggregate root implemented in a Domain-implementation prompt gets its own commit (entities + enums + Value Objects + aggregate doc). Cross-cutting structure and cross-cutting documentation each get their own commit, separate from any aggregate. See [../README.md](../README.md#implementation-mode-rules-binding-for-every-future-phase).

## History

- **2026-08-06** — Phase 0 (Planning Freeze) completed: roadmap ([roadmap.md](roadmap.md)), phase breakdown ([../phases/](../phases/)), documentation structure, Entity/CQRS/Persistence/Search strategy docs. See [../../tasks/2026-08-06/Task3_promotion-service-phase0-planning-freeze.md](../../tasks/2026-08-06/Task3_promotion-service-phase0-planning-freeze.md).
- **2026-08-06** — Phase 1 (Bootstrap Service) completed: cloned the 5-project Clean Architecture skeleton from Payment Service's foundation-phase precedent (`src/Services/Promotion/`), registered in `NovaCore.sln`, wired into `docker-compose.yml`/`docker-compose.override.yml`/`.env.template`/`scripts/postgres/init.sql`/the YARP gateway. `PromotionDbContext` carries Outbox/Inbox only — no aggregate exists yet. One verification build performed, 0 errors. See [../../tasks/2026-08-06/Task4_promotion-service-phase1-bootstrap-service.md](../../tasks/2026-08-06/Task4_promotion-service-phase1-bootstrap-service.md).
- **2026-08-06** — Phase 2.1 (Domain Foundation) completed: added `Events/`/`Metadata`/`Constants/` folders to `Promotion.Domain`; rewrote [../entities/entity-implementation-strategy.md](../entities/entity-implementation-strategy.md) into the grounded 12-step per-aggregate order. See [../../tasks/2026-08-06/Task5_promotion-service-phase2.1-domain-foundation.md](../../tasks/2026-08-06/Task5_promotion-service-phase2.1-domain-foundation.md).
- **2026-08-06** — Phase 2.1 (Campaign + Promotion aggregates, folded into the same sub-prompt number by the issuing prompt) completed: implemented both aggregates in full — 9 Campaign entities (`Campaign`, `CampaignSchedule`, `CampaignAudience`, `CampaignChannel`, `CampaignTag`, `CampaignAttachment`, `CampaignLocalization`, `CampaignBudget`, `CampaignApproval`), 13 Promotion entities + `PromotionMetadata` (reclassified as a Value Object, not an entity — see [../aggregates/promotion.md](../aggregates/promotion.md)), 7 enums, 5 Value Objects. Structural only, no business rule. No build performed, per the phase's own build policy. See [../../tasks/2026-08-06/Task6_promotion-service-phase2.1-campaign-promotion-aggregates.md](../../tasks/2026-08-06/Task6_promotion-service-phase2.1-campaign-promotion-aggregates.md).
- **2026-08-06** — Workflow updated: commit granularity rule added (see Progress Management Policy above) — every aggregate root gets its own commit going forward. The four Phase 2.1 commits (foundation, Campaign, Promotion, docs) were made retroactively under this rule before Phase 2.2 started.
- **2026-08-06** — Phase 2.2 (Coupon + Voucher aggregates) completed: implemented both aggregates in full — 8 Coupon entities (`Coupon`, `CouponBatch`, `CouponCode`, `CouponUsage`, `CouponReservation`, `CouponHistory`, `CouponVersion`, `CouponApproval`), 10 Voucher entities (`Voucher`, `VoucherWallet`, `VoucherIssue`, `VoucherBatch`, `VoucherReservation`, `VoucherRedemption`, `VoucherTransfer`, `VoucherHistory`, `VoucherExpiration`, `VoucherFreeze`), 5 enums (all values given explicitly this time, no invented taxonomy), 5 Value Objects (`Voucher` reuses the shared `Money` VO rather than duplicating it). Fixed one real defect found during this pass: `CouponCode` (entity) and `CouponCode` (Value Object) collide within the *same* namespace as `Coupon.cs`, which a plain global alias cannot resolve (C# same-namespace lookup wins over any `using`) — fixed with file-local aliases in both `Coupon.cs` and `CouponCode.cs`, plus a global `CouponCodeEntity` alias for external callers. Structural only, no business rule. No build performed. See [../../tasks/2026-08-06/Task7_promotion-service-phase2.2-coupon-voucher-aggregates.md](../../tasks/2026-08-06/Task7_promotion-service-phase2.2-coupon-voucher-aggregates.md).
- **2026-08-07** — Phase 2.3 (Loyalty + Reward + Distribution aggregates) completed: implemented all three aggregates in full — 9 Loyalty entities (`LoyaltyProgram`, `PointAccount`, `PointTransaction`, `PointLedger`, `PointExpiration`, `PointAdjustment`, `PointRule`, `PointPolicy`, `PointHistory`), 7 Reward entities (`RewardProgram`, `RewardDefinition`, `RewardDistribution`, `RewardExecution`, `RewardReservation`, `RewardClaim`, `RewardHistory`), 6 Distribution entities (`DistributionJob`, `DistributionBatch`, `DistributionItem`, `DistributionExecution`, `DistributionRetry`, `DistributionHistory`), 6 enums, 2 Value Objects (Loyalty only — Reward/Distribution's briefs gave no `ValueObjects` section, so their `Code` fields stay plain strings, flagged as a reconciliation). `RewardDistribution.Status` reuses `DistributionStatus` and `DistributionItem.RewardType` reuses `RewardType` — cross-aggregate enum reuse within the same phase, not duplicated. Structural only, no business rule. No build performed. See [../../tasks/2026-08-07/Task1_promotion-service-phase2.3-loyalty-reward-distribution-aggregates.md](../../tasks/2026-08-07/Task1_promotion-service-phase2.3-loyalty-reward-distribution-aggregates.md).
- **2026-08-07** — Phase 2.4 (Recommendation + Product Set + Gift + Approval + Validation + Audit — six groups bundled into one sub-prompt by the issuing prompt) completed: 5 Recommendation entities, 6 Product Set entities (`Quantity` VO reused from BuildingBlock, same as `Order.Domain`'s `OrderItem`), 6 Gift entities (`Quantity` VO reused again), 6 Approval entities (the structural foundation `CampaignApproval`/`CouponApproval` were deferring to — not wired together, since modifying prior aggregates was out of scope), 5 Validation entities + 4 Audit entities (both groups have **no aggregate root** — no Navigation/TenantId was given for any of their 9 entities combined, so all stayed plain `BaseEntity<Guid>` with no `ITenantEntity`, a first for this roadmap). 6 enums, 3 Value Objects (Reward/Distribution/Gift/Approval's pattern of "no ValueObjects section given" repeated for Gift and Approval this time). Every aggregate the original roadmap named is now implemented. No build performed. See [../../tasks/2026-08-07/Task2_promotion-service-phase2.4-recommendation-productset-gift-approval-validation-audit.md](../../tasks/2026-08-07/Task2_promotion-service-phase2.4-recommendation-productset-gift-approval-validation-audit.md). **Remaining in Phase 2 — next prompt (2.5, final) is Domain Review + single Domain build.**
