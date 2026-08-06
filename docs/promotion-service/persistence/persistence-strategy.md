# Promotion Service — Persistence Strategy

**Scope:** The future repository/persistence-service shape Promotion Service follows once Phase 3 (schema) and Phase 5 (CQRS) begin. Documentation only — no `DbContext`, repository, or persistence service exists yet. This commits Promotion Service to the platform's **current target shape**, binding in [../../conventions/persistence-coding-conventions.md](../../conventions/persistence-coding-conventions.md) — the Read/Write persistence-service pattern the rest of the platform is mid-migration toward (see [../../refactoring/persistence-refactor-plan.md](../../refactoring/persistence-refactor-plan.md)). Promotion Service is built directly in the target shape; it never adopts the older pattern other services are migrating away from.

## Roles

- **Write Repository** (`I{Aggregate}Repository`) — `IRepository<T, TId>`-based, used only for aggregate persistence (`Add`/`Update`/`Remove`, tracked entities). No querying logic beyond what a handler needs to load the aggregate it's about to mutate.
- **Read Repository** — not a separate type from the Write Repository in this platform's convention; read-only aggregate loads still go through the same `I{Aggregate}Repository` when the read needs a tracked, full aggregate graph (rare — most reads go through the Read Service instead).
- **Persistence Service** — the umbrella term for the Read/Write split below; a handler depends on the Persistence Service abstraction, never on `DbContext`/`IUnitOfWork` directly.
- **Read Service** (`I{Aggregate}ReadService`) — untracked, projection-based queries (`AsNoTracking`, direct-to-DTO `Select`), backing every Query handler. Owns pagination/filtering/sorting for that aggregate.
- **Specifications** — reusable, composable query predicates for filtering (e.g. active-only, date-range) shared between a Read Service's queries and, where relevant, the Search Service's non-ES fallback paths. Only added once a second query genuinely needs to share a predicate — not created speculatively.
- **Query Objects** — parameter objects a Read Service's method accepts when the query has more than 2-3 filter parameters, per the platform's existing convention for wide query surfaces — avoids long positional-parameter query methods.

## Transaction ownership

`IUnitOfWork`/`EfUnitOfWork` ownership stays with the Command handler (via the Write Repository + `IUnitOfWork.SaveChangesAsync`), never with the Persistence Service itself — matches [../../conventions/persistence-coding-conventions.md](../../conventions/persistence-coding-conventions.md) exactly, no divergence.

## Phase mapping

- **Phase 3** builds the schema only: `PromotionDbContext`, `IEntityTypeConfiguration<T>`, indexes/constraints, migration. No repository/service exists yet.
- **Phase 5** adds the Write Repository + Read/Write Persistence Service pair, but only for the aggregates in that CQRS pass's scope — every other aggregate has a mapped table and nothing else, same scope-discipline precedent as [../cqrs/cqrs-strategy.md](../cqrs/cqrs-strategy.md) documents.
