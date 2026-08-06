# Phase 3 — Persistence

## Purpose

Map the Phase 2 domain model to Postgres following [../persistence/persistence-strategy.md](../persistence/persistence-strategy.md) and the platform's binding [../../conventions/persistence-coding-conventions.md](../../conventions/persistence-coding-conventions.md) — `PromotionDbContext`, complete `IEntityTypeConfiguration<T>` per entity, indexes/unique constraints, and the initial EF migration. No repositories, no Read/Write persistence services yet (those are Phase 5).

## Expected Output

- `PromotionDbContext` with Outbox+Inbox tables, matching every other service's shape.
- Complete `IEntityTypeConfiguration<T>` for every Phase 2 entity (relational FKs, `OwnsOne` only for genuine Value Objects, full index/constraint coverage).
- [../indexes/](../indexes/) catalogue populated: one entry per index/unique constraint added, with the query it exists to serve.
- `dotnet ef migrations add InitialCreate` generated and committed.

## Build Verification

- `dotnet ef migrations add` succeeds with no warnings about unmapped types.
- `Promotion.Persistence` builds.
- Migration applies cleanly against an empty `promotion_db`.

## Completion Criteria

Every Phase 2 Domain entity has a table; every index/unique constraint the architect's design calls for exists and is documented in [../indexes/](../indexes/); migration is reviewed before commit.

## Blocked Items

Repositories and Read/Write persistence services are explicitly Phase 5's concern (schema-first, CQRS-second — same precedent Payment Service's foundation phase set, see [../../services/payment-service.md](../../services/payment-service.md#persistence-readwrite-services)).

## Dependencies

Phase 2 (Domain Model).
