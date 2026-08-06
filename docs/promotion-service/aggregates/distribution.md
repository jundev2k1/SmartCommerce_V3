# Distribution Aggregate

**Scope:** Domain-layer facts for the `DistributionJob` aggregate, implemented Phase 2.3. Structural only — no business rule (batching, dispatch, retry/backoff) is implemented yet.

## Aggregate boundary

`DistributionJob` (`Promotion.Domain/Entities/Distributions/DistributionJob.cs`) is the aggregate root — `AggregateRoot<Guid>`, `IAuditable`, `ITenantEntity`. It owns, via navigation + internal construction: `DistributionBatch`.

`DistributionItem`, `DistributionExecution`, `DistributionRetry`, `DistributionHistory` are **not** part of the navigation graph (no Navigation section given for any of them), so all four have a **public** `Create` and are related by id only (`BatchId`/`ItemId`/`ExecutionId`/`JobId` respectively) — same reconciliation as Loyalty/Reward.

## No Value Objects requested

Same as `RewardProgram` — no `ValueObjects` section was given, so `DistributionJob.Code` stays a plain `string`.

## `DistributionItem.RewardType` reuses the Reward aggregate's enum

`DistributionItem` has a `RewardType` field with no enum type specified, but the Reward aggregate (implemented in this same phase) already defines a `RewardType` enum with an exactly matching name and semantics. Reused directly rather than declaring a duplicate.

## Entities

| Entity | Shape | Notes |
|---|---|---|
| `DistributionJob` | `AggregateRoot<Guid>` | Code (plain string)/Name/Status/Strategy/ScheduledAt/StartedAt/CompletedAt |
| `DistributionBatch` | `BaseEntity<Guid>` | JobId/BatchNo/TotalItems/ProcessedItems/FailedItems |
| `DistributionItem` | `BaseEntity<Guid>` | BatchId/UserId/RewardType (reused)/ReferenceId/Status — public `Create` |
| `DistributionExecution` | `BaseEntity<Guid>` | ItemId/ExecutionKey/ExecutedAt — public `Create` |
| `DistributionRetry` | `BaseEntity<Guid>` | ExecutionId/RetryCount/LastRetryAt — public `Create` |
| `DistributionHistory` | `BaseEntity<Guid>` | JobId/Action/OperatorId — `CreatedAt` inherited, public `Create` |

## Enums

- `DistributionStatus` — Draft/Scheduled/Running/Paused/Completed/Cancelled/Failed (given explicitly) — also reused by `RewardDistribution.Status` in the Reward aggregate.
- `DistributionStrategy` — Broadcast/Segment/Import/Manual (given explicitly).

## Indexes (design only — written in Phase 3)

- `DistributionJob`: `(Code)` unique · `(Status)`
- `DistributionExecution`: `(ExecutionKey)` unique

## Reconciliation notes

No `EntityData`/`UpdateData` wrapper types created (Domain Rule 2).
