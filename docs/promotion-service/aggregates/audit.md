# Audit Entities

**Scope:** Domain-layer facts for the Audit entity group, implemented Phase 2.4. Structural only.

## No aggregate root — 4 independent, flat audit-log entities

Same reasoning as [validation.md](validation.md) — no Properties/Navigation/`TenantId` was given for any of these 4 entities, so all stay plain `BaseEntity<Guid>` with `IAuditable` only. Each is shaped exactly like every `*History` entity already implemented across this roadmap (`CouponHistory`, `VoucherHistory`, `PointHistory`, `RewardHistory`, `DistributionHistory`, `RecommendationHistory`, `ApprovalHistory` — `Id`/`{Scope}Id`/`Action`/`OperatorId`, `CreatedAt` inherited from `BaseEntity`) — the only difference is these are platform-level (not owned by, or navigated from, one specific aggregate root) rather than per-aggregate.

`ApprovalAudit` is a **distinct entity** from `ApprovalHistory` (in `Entities/Approvals/`) — the brief lists both as separate entities in separate groups (Approval's own `ApprovalHistory`, plus this group's `ApprovalAudit`), so both were implemented as written rather than treated as a duplicate.

## Entities

| Entity | Shape | Notes |
|---|---|---|
| `PromotionAudit` | `BaseEntity<Guid>` | AggregateId (generic, any Promotion-service aggregate)/Action/OperatorId |
| `RuleAudit` | `BaseEntity<Guid>` | RuleId (any rule-shaped entity)/Action/OperatorId |
| `ApprovalAudit` | `BaseEntity<Guid>` | WorkflowId/Action/OperatorId — distinct from `ApprovalHistory` (see above) |
| `ExecutionAudit` | `BaseEntity<Guid>` | ExecutionId (any execution-shaped entity)/Action/OperatorId |

All four: `Create(id, action, operatorId = null)`, public (no owning root), `CreatedAt` inherited from `BaseEntity`.

## Enums / Value Objects

None requested or created for this group.

## Reconciliation notes

No `EntityData`/`UpdateData` wrapper types created (Domain Rule 2). No `TenantId`/`ITenantEntity` added, since none was given.
