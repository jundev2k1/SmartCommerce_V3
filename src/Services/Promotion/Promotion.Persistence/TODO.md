# Persistence — TODO

**Phase:** 3 (Persistence). Bootstrap phase (1) only creates `PromotionDbContext` (Outbox+Inbox tables), `UnitOfWork`, the Outbox/Inbox adapters, and `PromotionBaseRepository` — all entity-independent.

Still to add, per [../../../docs/promotion-service/persistence/persistence-strategy.md](../../../docs/promotion-service/persistence/persistence-strategy.md):
- `Configs/{Entity}Config.cs` — one `IEntityTypeConfiguration<T>` per Phase 2 entity.
- `PromotionDbContext` — one `DbSet<T>` per entity, added alongside its config.
- `Storage/Migrations/` — the `InitialCreate` migration, generated once the schema is complete (Phase 3's own build-verification step).
- `Contexts/{Aggregate}/Repositories/`, `Contexts/{Aggregate}/Read/`, `Contexts/{Aggregate}/Write/` — only for the aggregates in-scope for a given CQRS pass (Phase 5), per the scope-discipline precedent in [../../../docs/promotion-service/cqrs/cqrs-strategy.md](../../../docs/promotion-service/cqrs/cqrs-strategy.md).
- `AddAuditHierarchy()` in `DependencyInjection.cs` — add an entry per `IAuditable` aggregate as it's implemented.
