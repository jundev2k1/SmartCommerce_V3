# Approval Aggregate

**Scope:** Domain-layer facts for the `ApprovalWorkflow` aggregate, implemented Phase 2.4. Structural only — no business rule (routing, escalation, SLA enforcement) is implemented yet.

## Aggregate boundary

`ApprovalWorkflow` (`Promotion.Domain/Entities/Approvals/ApprovalWorkflow.cs`) is the aggregate root — `AggregateRoot<Guid>`, `IAuditable`, `ITenantEntity`. It owns, via navigation + internal construction: `ApprovalStep`.

`ApprovalAssignment`, `ApprovalDecision`, `ApprovalComment`, `ApprovalHistory` are **not** part of the navigation graph (no Navigation section given for any of them) — all four get a public `Create`, related by `StepId` (or, for `ApprovalHistory`, `WorkflowId`) only.

## This is the structural foundation `CampaignApproval`/`CouponApproval` were deferring to

Phase 2.1's `CampaignApproval` and Phase 2.2's `CouponApproval` were both documented as minimal placeholders pending "a later Phase 2.x (Approval + Validation + Audit)." This is that phase. `ApprovalWorkflow`/`ApprovalStep`/etc. are the general-purpose workflow structure those two placeholders were pointing at — but **no wiring was added** connecting them (e.g. `CampaignApproval.WorkflowId`), since this prompt explicitly forbids modifying previously implemented aggregates. Wiring them together, if ever needed, is a future task.

## No Value Objects requested

Same as Gift/Reward/Distribution — `ApprovalWorkflow` has no `ValueObjects` section (it also has no `Code` field at all, unlike every other root this phase).

## Entities

| Entity | Shape | Notes |
|---|---|---|
| `ApprovalWorkflow` | `AggregateRoot<Guid>` | WorkflowType (plain string)/Status |
| `ApprovalStep` | `BaseEntity<Guid>` | WorkflowId/StepOrder/ApproverRole/Status (plain string, distinct from `ApprovalWorkflowStatus`) |
| `ApprovalAssignment` | `BaseEntity<Guid>` | StepId/UserId/AssignedAt — public `Create` |
| `ApprovalDecision` | `BaseEntity<Guid>` | StepId/Decision (plain string, no enum requested)/DecidedAt — public `Create` |
| `ApprovalComment` | `BaseEntity<Guid>` | StepId/Comment — `CreatedAt` inherited, public `Create` |
| `ApprovalHistory` | `BaseEntity<Guid>` | WorkflowId/Action/OperatorId — `CreatedAt` inherited, public `Create` |

## Enums

- `ApprovalWorkflowStatus` — Draft/Pending/Approved/Rejected/Cancelled (given explicitly).

## Indexes (design only — written in Phase 3)

`(Status)`

## Reconciliation notes

No `EntityData`/`UpdateData` wrapper types created (Domain Rule 2).
