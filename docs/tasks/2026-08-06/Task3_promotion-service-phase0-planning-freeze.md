# Task 3: Promotion Service — Phase 0 Planning Freeze

**Status:** Done (documentation only, per the task's own scope)
**Category:** Planning/roadmap freeze for a brand-new service — no code, no projects, no entities

## What was done

Froze the 7-phase implementation roadmap and internal documentation structure for `SmartEcommerce.PromotionService`, a new platform-wide Promotion Engine (not an Order Service module) already designed by the system architect. This is Phase 0 of 7 — planning only, per explicit instruction: no project created, no service cloned, no entities generated, no source code touched, nothing built.

Created `docs/promotion-service/`:
- `README.md` — entry point, binding implementation-mode rules (never redesign, never add entities unrequested, never infer business logic, never merge/skip phases), structure table.
- `planning/PROGRESS.md` — progress tracker (0/7 implementation phases, current phase, status, completion %).
- `planning/roadmap.md` — 7-phase overview, dependency graph, explicit exclusion of real business logic from Phases 1-7.
- `phases/phase-{1..7}.md` — one file per phase (Bootstrap Service, Domain Model, Persistence, Search Integration, CQRS Skeleton, Infrastructure Integration, Migration Preparation), each with Purpose/Expected Output/Build Verification/Completion Criteria/Blocked Items/Dependencies.
- `entities/entity-implementation-strategy.md` — the fixed 11-step per-aggregate order (Entity → Properties → Navigation Properties → Enums → Value Objects → Owned Types → Indexes → Unique Constraints → Relationships → Aggregate Boundaries → Future TODO), binding for Phase 2.
- `cqrs/cqrs-strategy.md`, `persistence/persistence-strategy.md`, `search/search-strategy.md` — future strategy docs (Commands/Queries/Validators/Handlers/DTOs/Contracts/Persistence Services/Repositories/Endpoints; Read/Write Repository/Persistence Service/Read Service/Specifications/Query Objects; Documents/Index Config/Indexer/Search Service/Autocomplete/Fuzzy Search), each explicitly committing to the platform's *existing* conventions rather than inventing a new pattern.
- `architecture/`, `aggregates/`, `value-objects/`, `enums/`, `indexes/`, `integration/`, `tasks/` — placeholder `README.md`s stating purpose only, populated starting whichever phase actually produces that content (Phase 2/3/6). `tasks/README.md` explicitly points back to this repo-wide `docs/tasks/` convention instead of duplicating it.

Linked the new tree from `docs/README.md` (new "Promotion Service (planning)" subsection under Services).

## Objective

Have the full roadmap + doc skeleton in place so Phase 1 (Bootstrap Service) can start from a clean, unambiguous brief without re-deriving scope, ordering, or conventions each phase.

## Current state (grounded findings)

- No `src/Services/Promotion/` directory, no `Promotion` entry in `NovaCore.sln`, no `PROMOTION_*` env vars, no `promotion_db`, no Gateway route — confirmed via repo search before writing any doc.
- The only pre-existing Promotion-adjacent reference in the repo is this task's own new doc tree; nothing else in `docs/` mentioned Promotion Service before this task.
- Followed the same shape Payment Service's foundation-phase docs already established (`docs/services/payment-service.md`, `docs/tasks/2026-08-06/Task1_paymentservice-foundation.md`) as the precedent for how a brand-new service's scope/postponed-work sections read.

## Scope

**Built this phase:** planning documents only, per the list above.

**Explicitly not built (see `docs/promotion-service/phases/`):**
- Any project, entity, migration, repository, endpoint, or integration event — all deferred to Phases 1-7.
- Any actual business rule (discount calculation, eligibility evaluation, redemption workflow) — explicitly out of scope for the entire 7-phase roadmap, requires its own future prompt after Phase 7.

## Dependencies

None — this is the first task for Promotion Service. Phase 1 (Bootstrap Service) depends on this freeze being complete.

## Estimated complexity

Small (documentation-only session, ~20 new files, no code).

## Risks

- The roadmap assumes the architect's design (aggregates, business rules) will be supplied ahead of or during Phase 2 — none of it exists in this repo yet, so Phase 2 cannot start until that design is actually provided.
