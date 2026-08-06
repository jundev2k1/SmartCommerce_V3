# Migration — TODO

**Phase:** 7 (Migration Preparation), with the first (`InitialCreate`) migration actually generated during Phase 3 (Persistence) as that phase's own build-verification step.

Per the Phase 1 (Bootstrap Service) build policy: **no EF Core migration is generated in this phase.** `PromotionDbContext` currently has no aggregate `DbSet<T>` to migrate — only Outbox/Inbox tables, which every other service already migrates as part of its own `InitialCreate`, so there is nothing meaningful to generate standalone here yet.

When Phase 3 completes the schema, run `dotnet ef migrations add InitialCreate` from `Promotion.API` (see `PromotionDbContextFactory.cs` for the design-time context). Phase 7 then re-verifies the full migration set against a fresh database as its own readiness check — see [../../../../docs/promotion-service/phases/phase-7-migration-preparation.md](../../../../docs/promotion-service/phases/phase-7-migration-preparation.md).
