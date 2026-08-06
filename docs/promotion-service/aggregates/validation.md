# Validation Entities

**Scope:** Domain-layer facts for the Validation entity group, implemented Phase 2.4. Structural only — no business rule (rule evaluation, simulation execution) is implemented yet.

## No aggregate root this time

Unlike every prior "aggregate" group, the brief gave **no Properties/Navigation section for any of these 5 entities** — no `TenantId`, no `Status`/lifecycle field, nothing marking one of them as an `AggregateRoot`. Rather than inventing a root shape (adding `TenantId`/`IAuditable` root fields not given, or arbitrarily promoting one entity to `AggregateRoot<Guid>`), all 5 stay plain `BaseEntity<Guid>` with `IAuditable` only — no `ITenantEntity`. This is a deliberate, literal reading of the brief's omission, not an oversight on this pass's part; flagging for architect confirmation before Persistence (Phase 3) locks in the schema.

Structurally, the 5 entities form two independent FK chains rather than one aggregate:
- `PromotionValidationPolicy` → `PromotionValidationResult` (via `PolicyId`)
- `PromotionSimulation` → `PromotionSimulationScenario` (via `SimulationId`) → `PromotionSimulationResult` (via `ScenarioId`)

All `Create` factories are public (no owning root to restrict construction to `internal`).

## Entities

| Entity | Shape | Notes |
|---|---|---|
| `PromotionValidationPolicy` | `BaseEntity<Guid>` | Name/RuleType/Configuration (opaque string blob)/Priority |
| `PromotionValidationResult` | `BaseEntity<Guid>` | PolicyId/Status/Message |
| `PromotionSimulation` | `BaseEntity<Guid>` | Name/CreatedBy — `CreatedAt` inherited, not redeclared |
| `PromotionSimulationScenario` | `BaseEntity<Guid>` | SimulationId/Name/Input (opaque string blob) |
| `PromotionSimulationResult` | `BaseEntity<Guid>` | ScenarioId/Output/Status |

## Enums / Value Objects

None requested or created for this group.

## Reconciliation notes

No `EntityData`/`UpdateData` wrapper types created (Domain Rule 2). No `TenantId`/`ITenantEntity` added to any of the 5 entities, since none was given — see "No aggregate root this time" above.
