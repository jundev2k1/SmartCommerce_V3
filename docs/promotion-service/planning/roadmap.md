# Promotion Service — Roadmap

**Scope:** The 7 implementation phases, in fixed linear order. Each phase is strictly dependent on the one before it — no phase starts before its dependency's Completion Criteria are met, and no phase's work is pulled forward into an earlier one. Full detail per phase lives in [../phases/](../phases/); this file is the one-screen overview.

| # | Phase | One-line purpose | Detail |
|---|---|---|---|
| 0 | Planning Freeze | Freeze the roadmap and internal doc structure before any implementation. | *this tree* |
| 1 | Bootstrap Service | Stand up the empty 5-layer Clean Architecture skeleton, wired into the solution and cross-cutting infra. | [phase-1-bootstrap-service.md](../phases/phase-1-bootstrap-service.md) |
| 2 | Domain Model | Implement the architect's aggregates/entities/enums/Value Objects, no persistence concerns. | [phase-2-domain-model.md](../phases/phase-2-domain-model.md) |
| 3 | Persistence | Map the domain model to Postgres — DbContext, EF configs, indexes, initial migration. | [phase-3-persistence.md](../phases/phase-3-persistence.md) |
| 4 | Search Integration | Elasticsearch documents, indexer, search service, autocomplete, fuzzy search. | [phase-4-search-integration.md](../phases/phase-4-search-integration.md) |
| 5 | CQRS Skeleton | Commands/Queries/Validators/Handlers/DTOs/Repositories/Endpoints for the in-scope aggregates. | [phase-5-cqrs-skeleton.md](../phases/phase-5-cqrs-skeleton.md) |
| 6 | Infrastructure Integration | Kafka Outbox/Inbox, integration event contracts, first real cross-service wiring. | [phase-6-infrastructure-integration.md](../phases/phase-6-infrastructure-integration.md) |
| 7 | Migration Preparation | Production-readiness pass: docs, seed data, final wiring, readiness checklist. | [phase-7-migration-preparation.md](../phases/phase-7-migration-preparation.md) |

## Dependency graph

```
Phase 0 → Phase 1 → Phase 2 → Phase 3 → Phase 4 → Phase 5 → Phase 6 → Phase 7
                                              persistence-strategy →┘
                                (search-strategy feeds Phase 4, cqrs-strategy feeds Phase 5)
```

## What this roadmap explicitly excludes

Real business logic — promotion evaluation rules, discount calculation, eligibility engines, redemption workflows — is **not** part of Phases 1-7. Those phases build the structural foundation only (mirrors the precedent Payment Service's foundation phase set — see [../../services/payment-service.md](../../services/payment-service.md)'s "Planned phases" section). Business-rule implementation requires its own future prompt(s) once Phase 7 closes.
