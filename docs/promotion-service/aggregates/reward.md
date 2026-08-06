# Reward Aggregate

**Scope:** Domain-layer facts for the `RewardProgram` aggregate, implemented Phase 2.3. Structural only — no business rule (eligibility, dispatch, claim enforcement) is implemented yet.

## Aggregate boundary

`RewardProgram` (`Promotion.Domain/Entities/Rewards/RewardProgram.cs`) is the aggregate root — `AggregateRoot<Guid>`, `IAuditable`, `ITenantEntity`. It owns, via navigation + internal construction: `RewardDefinition`, `RewardDistribution`.

`RewardExecution`, `RewardReservation`, `RewardClaim`, `RewardHistory` are **not** part of the navigation graph (no Navigation section given for any of them), so all four have a **public** `Create` and are related by id only. None of them reference a `RewardId` pointing at a dedicated "Reward" entity — no such entity exists in this pass (only `RewardProgram`/`RewardDefinition`/`RewardDistribution` do); `RewardId` is kept as a loose `Guid` reference for whatever "issued reward" concept a later phase formalizes.

## No Value Objects requested (a first for this roadmap)

Unlike every prior aggregate (Campaign, Promotion, Coupon, Voucher, Loyalty), the brief gave `RewardProgram` **no `ValueObjects` section**. `Code` is kept as a plain `string`, not wrapped in a `RewardProgramCode`-shaped VO — respecting the literal omission rather than inventing an unrequested type. Flagging this explicitly since it breaks an otherwise 100%-consistent pattern; if it turns out to be a brief oversight rather than a deliberate simplification, adding the VO later is a pure-additive, non-breaking change.

## `RewardDistribution.Status` reuses `DistributionStatus`

`RewardDistribution` has a `Status` field with no enum type specified, but the Distribution aggregate (implemented in this same phase) defines a `DistributionStatus` enum whose semantics match exactly, and `RewardDistribution.DistributionJobId` already ties the two together contextually. Reused directly rather than declaring a duplicate enum.

## Entities

| Entity | Shape | Notes |
|---|---|---|
| `RewardProgram` | `AggregateRoot<Guid>` | Code (plain string)/Name/Description/Status/StartTime/EndTime |
| `RewardDefinition` | `BaseEntity<Guid>` | ProgramId/RewardType/Configuration (opaque string blob) |
| `RewardDistribution` | `BaseEntity<Guid>` | ProgramId/DistributionJobId/Status (`DistributionStatus`, reused)/ScheduledAt/ExecutedAt |
| `RewardExecution` | `BaseEntity<Guid>` | DistributionId/UserId/ExecutionKey/Status/ExecutedAt — public `Create` |
| `RewardReservation` | `BaseEntity<Guid>` | RewardId/UserId/ReservedAt/ExpiredAt — public `Create` |
| `RewardClaim` | `BaseEntity<Guid>` | RewardId/UserId/ClaimedAt — public `Create` |
| `RewardHistory` | `BaseEntity<Guid>` | RewardId/Action/OperatorId — `CreatedAt` inherited, public `Create` |

## Enums

- `RewardProgramStatus` — Draft/Active/Paused/Expired/Archived (given explicitly).
- `RewardType` — Coupon/Voucher/Point/Gift/Cashback (given explicitly) — also reused by `DistributionItem.RewardType` in the Distribution aggregate.

## Indexes (design only — written in Phase 3)

- `RewardProgram`: `(Code)` unique · `(Status)`
- `RewardExecution`: `(ExecutionKey)` unique · `(UserId)`

## Reconciliation notes

No `EntityData`/`UpdateData` wrapper types created (Domain Rule 2).
