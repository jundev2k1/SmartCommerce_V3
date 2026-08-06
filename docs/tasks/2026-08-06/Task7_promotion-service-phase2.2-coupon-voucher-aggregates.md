# Task 7: Promotion Service — Phase 2.2 Coupon + Voucher Aggregates

**Status:** Done (Domain layer only, structural, per the prompt's own scope)
**Category:** Domain-layer entity implementation, no business logic, no build; also introduced a new commit-granularity workflow rule

## What was done

**Workflow change (requested first, before the Phase 2.2 implementation):** added a permanent rule to `docs/promotion-service/README.md` and `docs/promotion-service/planning/PROGRESS.md` — every aggregate root implemented in a Domain-implementation prompt now gets its own commit, separate from cross-cutting structure/documentation commits. Applied retroactively to the still-uncommitted Phase 2.1 output first (4 commits: Domain foundation structure, Campaign aggregate, Promotion aggregate, cross-cutting docs), then to this phase's own output.

**Coupon aggregate** (`Promotion.Domain/Entities/Coupons/`) — 8 entities: `Coupon` (root, `AggregateRoot<Guid>`/`IAuditable`/`ITenantEntity`, with `Promotion`/`Campaign`/`Batch` navigation *objects* — unlike Campaign/Promotion, this phase's brief explicitly requested object references, not just FK ids), `CouponBatch` (public `Create`, independent lifecycle), `CouponCode` (entity — individually issued codes, public `Create`), `CouponUsage`, `CouponReservation`, `CouponHistory`, `CouponVersion` (all four owned/`internal`-constructed), `CouponApproval` (structural placeholder, deferred workflow). 3 enums (`CouponStatus`, `CouponVisibility`, `CouponType` — all values given explicitly this time, no invented taxonomy), 3 Value Objects (`CouponCode`, `CouponPeriod`, `CouponUsageLimit`). Full detail: [docs/promotion-service/aggregates/coupon.md](../../promotion-service/aggregates/coupon.md).

**Voucher aggregate** (`Promotion.Domain/Entities/Vouchers/`) — 10 entities: `Voucher` (root — FK ids only for Promotion/Campaign, no navigation object, a deliberate difference from Coupon matching what was literally requested), `VoucherWallet` (public `Create`, keyed by `UserId` not `VoucherId` — independent of any single voucher), `VoucherIssue`, `VoucherReservation`, `VoucherRedemption`, `VoucherTransfer`, `VoucherHistory` (owned/`internal`), `VoucherBatch`/`VoucherExpiration`/`VoucherFreeze` (public `Create`, FK-only, not in the requested Navigation list). 2 enums (`VoucherStatus`, `VoucherType`, values given explicitly), 2 new Value Objects (`VoucherCode`, `VoucherPeriod`) plus the shared `BuildingBlock.Domain.ValueObjects.Money` reused directly for every Money-valued field (no new local Money VO, matching Payment Service's precedent). Full detail: [docs/promotion-service/aggregates/voucher.md](../../promotion-service/aggregates/voucher.md).

**Defect found and fixed:** `CouponCode` is both an entity and a Value Object, and — unlike the earlier `Promotion`/`PromotionEntity` collision — a plain global alias was not sufficient here, because `Coupon.cs` lives in the *same* namespace as the `CouponCode` entity (`Entities.Coupons`); C# resolves a bare identifier against same-namespace declarations before any `using`/global-using import is even considered, so `Coupon.Code`'s declared type would have silently bound to the entity instead of the Value Object. Fixed with file-local aliases (`using CouponCodeVo = ...`) in both `Coupon.cs` and `CouponCode.cs`, plus a global `CouponCodeEntity` alias in `GlobalUsings.cs` for any future file outside that namespace.

**Reconciliations, consistent with Task 5/6's precedent:** no `EntityData`/`UpdateData`/`TranslationData` wrapper types created (Domain Rule 2 — flat parameters only); `VoucherBatch`'s fields were inferred from `CouponBatch`'s parallel shape since the brief gave every other entity a Fields subsection except this one (flagged explicitly in `voucher.md`); status-transition method names (`MarkIssued`, `MarkReserved`, `MarkRedeemed`) were chosen to avoid colliding with the collection-add methods (`AddIssue`, `AddReservation`, `AddRedemption`) sharing the same underlying verb.

## Objective

Add two more complete, convention-correct aggregates without touching Campaign/Promotion, while establishing the new per-aggregate-root commit convention for the rest of the roadmap.

## Current state (grounded findings)

- `Promotion.Domain` now has 22 (Campaign+Promotion) + 18 (Coupon+Voucher) = 40 entity files, 1 Metadata VO, 12 enums, 10 Value Objects (`Money` reused, not counted as new). Every `Create`/`internal` call site was cross-checked argument-by-argument against its target signature, same discipline as Task 6 — not build-verified (forbidden until Phase 2.6).
- Git history now has 4 additional commits for this phase's own output (see below), plus the 4 retroactive Phase 2.1 commits made at the start of this task.

## Scope

**Built this prompt:** the two aggregates in full, per the entity/enum/VO lists above, plus the workflow documentation change.

**Explicitly not built:** any Persistence/Infrastructure/CQRS/API/Search code; any business rule; any build; no modification to Campaign/Promotion.

## Dependencies

Phase 2.1 (Task 6). Phase 2.3 (Loyalty + Reward + Distribution) depends on this pair existing as an additional pattern reference (particularly `CouponBatch`/`VoucherWallet`'s "independent, publicly-constructed entity" shape, useful for Reward/Distribution's likely similar needs).

## Estimated complexity

Large (28 new Domain types across 2 aggregates, one real namespace-collision defect caught and fixed mid-implementation, no build to catch mistakes).

## Risks

- Not build-verified — same caveat as Task 6.
- `CouponType`/`VoucherType` enum *values* were given explicitly this time (no invention needed), but `CampaignType`/`PromotionType` from Phase 2.1 remain structural placeholders still needing architect confirmation — unchanged risk, just restating it since both live in the same `Enums/README.md` catalogue now.
