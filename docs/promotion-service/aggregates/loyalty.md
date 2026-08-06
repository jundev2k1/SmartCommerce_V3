# Loyalty Aggregate

**Scope:** Domain-layer facts for the `LoyaltyProgram` aggregate, implemented Phase 2.3. Structural only — no business rule (earn/spend calculation, tier evaluation, expiry sweeping) is implemented yet.

## Aggregate boundary

`LoyaltyProgram` (`Promotion.Domain/Entities/Loyalty/LoyaltyProgram.cs`) is the aggregate root — `AggregateRoot<Guid>`, `IAuditable`, `ITenantEntity`. It owns, via navigation + internal construction (Rule 3): `PointRule`, `PointPolicy`, `PointAccount`.

`PointTransaction`, `PointLedger`, `PointExpiration`, `PointAdjustment`, `PointHistory` are **not** part of this aggregate's navigation graph — the brief gave `PointAccount` (and every other non-root entity) no "Navigation" section at all, so every one of these is related by id only (`AccountId` or, for `PointLedger`, `TransactionId`) with a **public** `Create` — same reconciliation rule used for Coupon's `CouponBatch`/`CouponCode`/`CouponApproval`: only entities that appear in an explicit Navigation list of their logical owner get `internal` construction.

## Entities

| Entity | Shape | Notes |
|---|---|---|
| `LoyaltyProgram` | `AggregateRoot<Guid>` | Code/Name/Description/Status/StartTime/EndTime/IsDefault/IsEnabled — **no TimeZone property**, unlike every prior aggregate |
| `PointAccount` | `BaseEntity<Guid>` | UserId/ProgramId/AvailablePoints/PendingPoints/ExpiredPoints/LifetimeEarned/LifetimeSpent |
| `PointTransaction` | `BaseEntity<Guid>` | AccountId/Type/Source/ReferenceId/Points/BalanceAfter — public `Create` |
| `PointLedger` | `BaseEntity<Guid>` | TransactionId/Debit/Credit/Balance — public `Create` |
| `PointExpiration` | `BaseEntity<Guid>` | AccountId/Points/ExpiredAt — public `Create` |
| `PointAdjustment` | `BaseEntity<Guid>` | AccountId/Reason/Points/OperatorId/AdjustedAt — public `Create` |
| `PointRule` | `BaseEntity<Guid>` | ProgramId/RuleType (plain string, no enum requested)/Priority/IsEnabled |
| `PointPolicy` | `BaseEntity<Guid>` | ProgramId/PolicyType/Configuration (opaque string blob) |
| `PointHistory` | `BaseEntity<Guid>` | AccountId/Action/OperatorId — `CreatedAt` inherited, public `Create` |

## Enums

- `LoyaltyProgramStatus` — Draft/Active/Paused/Expired/Archived (given explicitly).
- `PointTransactionType` — Earn/Spend/Refund/Expire/Adjust/Reward/Promotion (given explicitly; `Promotion` is an enum member, not a bare type reference, so it doesn't trigger the root-namespace collision).

## Value Objects

- `LoyaltyProgramCode` — same shape as `CampaignCode`.
- `LoyaltyPeriod` — same shape as `CampaignPeriod`, kept for shape-consistency even though `LoyaltyProgram` has no `TimeZone` field to pair it with; reserved, not yet consumed.

## Indexes (design only — written in Phase 3)

- `LoyaltyProgram`: `(Code)` unique · `(Status)` · `(TenantId, Code)` unique
- `PointAccount`: `(UserId, ProgramId)` unique
- `PointTransaction`: `(AccountId)` · `(ReferenceId)` · `(Type)`

## Reconciliation notes

No `EntityData`/`UpdateData` wrapper types created (Domain Rule 2). `LoyaltyProgram` omits `TimeZone` entirely — respected literally rather than adding an unrequested field to match the other aggregates' shape.
