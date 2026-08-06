# Task 1: Promotion Service — Phase 2.3 Loyalty + Reward + Distribution Aggregates

**Status:** Done (Domain layer only, structural, per the prompt's own scope)
**Category:** Domain-layer entity implementation, no business logic, no build

## What was done

Implemented three aggregates in one prompt, following the same conventions established in Phase 2.1/2.2 (Task 5-7, `2026-08-06`). No modification to any previously implemented aggregate (Campaign/Promotion/Coupon/Voucher).

**Loyalty aggregate** (`Promotion.Domain/Entities/Loyalty/`) — 9 entities: `LoyaltyProgram` (root, `AggregateRoot<Guid>`/`IAuditable`/`ITenantEntity`, owns `PointRule`/`PointPolicy`/`PointAccount` via navigation), `PointAccount` (owned, `internal` `Create`), `PointTransaction`, `PointLedger`, `PointExpiration`, `PointAdjustment`, `PointHistory` (all public `Create` — no Navigation section was given for `PointAccount` itself, so none of its children are owned-collection navigated), `PointRule`, `PointPolicy` (both owned). 2 enums (`LoyaltyProgramStatus`, `PointTransactionType`), 2 Value Objects (`LoyaltyProgramCode`, `LoyaltyPeriod`). `LoyaltyProgram` is the first aggregate in this roadmap with **no `TimeZone` property** — respected literally rather than added to match the others. Full detail: [docs/promotion-service/aggregates/loyalty.md](../../promotion-service/aggregates/loyalty.md).

**Reward aggregate** (`Promotion.Domain/Entities/Rewards/`) — 7 entities: `RewardProgram` (root, owns `RewardDefinition`/`RewardDistribution`), `RewardDefinition` (owned), `RewardDistribution` (owned — `Status` reuses the Distribution aggregate's `DistributionStatus` enum rather than declaring a duplicate), `RewardExecution`, `RewardReservation`, `RewardClaim`, `RewardHistory` (all public `Create`, no Navigation section given). 2 enums (`RewardProgramStatus`, `RewardType`). **No Value Objects section was given** — `RewardProgram.Code` stays a plain `string`, breaking an otherwise consistent pattern across every prior aggregate; flagged explicitly rather than inventing a `RewardProgramCode` VO. Full detail: [docs/promotion-service/aggregates/reward.md](../../promotion-service/aggregates/reward.md).

**Distribution aggregate** (`Promotion.Domain/Entities/Distributions/`) — 6 entities: `DistributionJob` (root, owns `DistributionBatch`), `DistributionBatch` (owned), `DistributionItem` (public `Create` — `RewardType` reuses the Reward aggregate's enum), `DistributionExecution`, `DistributionRetry`, `DistributionHistory` (all public `Create`, no Navigation section given). 2 enums (`DistributionStatus`, `DistributionStrategy`). Same as Reward, no Value Objects section given — `DistributionJob.Code` stays plain `string`. Full detail: [docs/promotion-service/aggregates/distribution.md](../../promotion-service/aggregates/distribution.md).

**Cross-aggregate enum reuse (new pattern this phase):** `RewardDistribution.Status` (`DistributionStatus`) and `DistributionItem.RewardType` (`RewardType`) each reuse an enum defined in the *other* aggregate implemented in this same prompt, rather than declaring near-duplicate enums — both field names/semantics matched exactly and the entities are already linked by an explicit FK (`DistributionJobId`). No prior phase had this situation (Campaign/Promotion/Coupon/Voucher's cross-references were all by id only, never by shared enum).

**Reconciliations, consistent with Task 5-7's precedent:** no `EntityData`/`UpdateData` wrapper types created (Domain Rule 2); every non-root entity without an explicit "Navigation" section in the brief got a **public** `Create` (not owned by any aggregate root), extending the rule Task 7 established for `CouponBatch`/`CouponApproval` to a much larger share of entities this time (14 of 22 new entities are public-`Create`, since only `LoyaltyProgram`/`RewardProgram`/`DistributionJob` had non-empty Navigation lists at all).

## Objective

Add three more complete, convention-correct aggregates without touching Campaign/Promotion/Coupon/Voucher, continuing the per-aggregate-root commit convention.

## Current state (grounded findings)

- `Promotion.Domain` now has 40 (prior) + 22 (this phase) = 62 entity files, 1 Metadata VO, 18 enums, 12 Value Objects. Every `Create`/`internal` call site cross-checked argument-by-argument against its target signature — not build-verified (forbidden until Phase 2.6).
- This is the first task filed under a new date folder (`2026-08-07`) since the prior session's work (Tasks 1-7) all fell on `2026-08-06` — per `docs/tasks/README.md`'s convention, task numbering restarts at 1 in a new date folder.

## Scope

**Built this prompt:** the three aggregates in full, per the entity/enum/VO lists above.

**Explicitly not built:** any Persistence/Infrastructure/CQRS/API/Search code; any business rule; any build; no modification to any previously implemented aggregate.

## Dependencies

Phase 2.2 (Task 7, `2026-08-06`). Phase 2.4 (Recommendation + Product Set + Gift) depends on this trio existing as an additional pattern reference.

## Estimated complexity

Large (30 new Domain types across 3 aggregates, no build to catch mistakes).

## Risks

- Not build-verified — same caveat as every prior Domain-implementation task this roadmap.
- `RewardProgram.Code`/`DistributionJob.Code` being plain strings (no VO) is a real inconsistency with every other aggregate's `Code` field — worth architect confirmation before Persistence (Phase 3) locks in the column type.
- `LoyaltyProgram`'s missing `TimeZone` property is likewise worth flagging for confirmation, since every other time-windowed aggregate has one.
