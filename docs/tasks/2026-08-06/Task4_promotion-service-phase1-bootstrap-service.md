# Task 4: Promotion Service — Phase 1 Bootstrap Service

**Status:** Done (structural skeleton only, per the task's own scope)
**Category:** New service scaffold — architecture clone, no business logic, no entities

## What was done

Cloned the production-ready service architecture into a new `SmartEcommerce.PromotionService` skeleton, using Payment Service's foundation phase (`docs/services/payment-service.md`, the most recent "brand-new service" precedent) as the direct template, cross-checked against User Service for the fuller Search/gRPC/Consumers/BackgroundJobs folder shape referenced in Phase 1's brief.

**Created** `src/Services/Promotion/` — 5 projects (`Promotion.Domain/Application/Infrastructure/Persistence/API`), same reference chain and package set as Payment's csproj files line-for-line (namespace substituted):
- **Domain**: `Promotion.Domain.csproj` + `GlobalUsings.cs` only — no entity/enum/Value Object exists yet (Phase 2).
- **Application**: `DependencyInjection.cs` (`AddApplication`: MediatR, ApplicationBehaviors, Mapster, FluentValidation — all currently scan zero handlers/validators).
- **Infrastructure**: `DependencyInjection.cs` (`AddInfrastructure`: AppLogger, Redis cache, Idempotency, Outbox/Inbox cleanup jobs, audit metadata provider, application event dispatcher, Kafka messaging client, Inbox/Outbox infrastructure) — matches Payment's exact wiring, service name `"promotion-service"`.
- **Persistence**: `PromotionDbContext` (Outbox+Inbox tables only, no aggregate `DbSet<T>` yet), `UnitOfWork`, `OutboxStore`/`InboxStore` adapters (byte-for-byte clones of Payment's, namespace-only diff), `PromotionBaseRepository<TEntity[,TId]>` generic base, `DependencyInjection.cs` (`AddPersistence`: DbContext, application services, repository scan, UnitOfWork, Outbox/Inbox, an empty `ConfigureAuditHierarchy` block ready for Phase 2 entries).
- **API**: `Program.cs`/`DependencyInjection.cs`/`ApplicationPipeline.cs` wired via `AddBuildingBlockWeb`/`UseBuildingBlockWeb` (no hand-rolled Swagger/CORS/Carter/exception-handler code), `PromotionDbContextFactory` (design-time `dotnet ef` support), `appsettings.json`/`appsettings.Development.json`, `Dockerfile` (multi-stage, matches Payment's exactly). No `SeedDatabase()` step — no seed data exists yet.

**Folder structure prepared** (empty, `.gitkeep`-marked) for every category the phase brief named: `Entities/Enums/ValueObjects` (Domain); `Features/Abstractions-Persistence/Abstractions-Search/Abstractions-Services/Common` (Application); `Messaging-Consumers/BackgroundJobs/GrpcClients` (Infrastructure); `Configs/Search/Storage-Migrations/Storage-Seeders` (Persistence); `Endpoints/GrpcServices` (API); `BuildingBlock.Contract/Events/Promotion/` (shared integration-event contracts location, matching the existing `Events/{Service}/` convention — this is where "Contracts" resolved to, not a per-service folder).

**TODO documents** (the 6 categories requested): `Promotion.Domain/TODO.md`, `Promotion.Persistence/TODO.md`, `Promotion.Persistence/Search/TODO.md`, `Promotion.Application/Features/TODO.md` (CQRS), `Promotion.Infrastructure/TODO.md`, `Promotion.Persistence/Storage/Migrations/TODO.md` — each cross-links to the matching `docs/promotion-service/` strategy doc and phase brief.

**Solution/cross-cutting wiring**: `dotnet sln add --solution-folder Services/Promotion` for all 5 projects; `promotion-api` service block added to `docker-compose.yml` (build+depends_on+networks) and `docker-compose.override.yml` (ports/env/healthcheck, byte-for-byte structural clone of Payment's block); `promotion_db` added to `scripts/postgres/init.sql`; `PROMOTION_*` key block added to `.env.template` (values intentionally empty, matching template convention — no `.env` file exists in this checkout to update); `"Promotion"` route (`/api/promotion/`, `RequireAuth: true`) added to `src/ApiGateways/YarpApiGateway/appsettings.json`, alphabetically between Product and User.

**Build**: exactly one `dotnet build src/Services/Promotion/Promotion.API/Promotion.API.csproj -c Debug` — succeeded, 0 errors (4 pre-existing `NU1510` warnings from `BuildingBlock.Application`, unrelated to this change). Per the phase's build policy, no further build will run until the entire Domain phase (Phase 2) is complete.

## Objective

Have a structurally identical, compiling skeleton so Phase 2 (Domain Model) can implement aggregates directly against established conventions without any architectural decisions left open.

## Current state (grounded findings)

- No `.env` file exists in this checkout (git-ignored, never committed) — `PROMOTION_*` keys were added to `.env.template` only, matching every other service's template entries (empty values).
- Public debug port conventions for other services (`PAYMENT_PUBLIC_HTTP_PORT=5109` is the only one documented in-repo) live only in the untracked `.env` — `PROMOTION_PUBLIC_HTTP_PORT` was added with an empty value, no number invented. `Promotion.API/DependencyInjection.cs`'s `ContactUrl` uses `http://localhost:5110` as a Swagger-contact-only placeholder (same non-binding role Payment's `5109`/other services' placeholder `5101` values already play — not read from config anywhere).
- No EF Core migration was generated — `PromotionDbContext` has no aggregate to migrate yet; the phase's build policy explicitly forbids generating one until Phase 3.
- No Docker Compose run was performed — build-only verification, per the phase's stated policy ("Bootstrap Phase → Build exactly ONE time after cloning").

## Scope

**Built this phase:** structural skeleton only, per the list above.

**Explicitly not built:**
- Any entity, enum, Value Object, aggregate (Phase 2).
- Any `IEntityTypeConfiguration<T>`, migration, repository, Read/Write service (Phase 3/5).
- Any search document/indexer/index (Phase 4).
- Any command/query/handler/endpoint (Phase 5).
- Any integration event contract/consumer, background job, gRPC client/server (Phase 6).

## Dependencies

Phase 0 (Planning Freeze). Phase 2 (Domain Model) depends on this bootstrap.

## Estimated complexity

Medium (structural clone across 5 projects + solution/Docker/env/Gateway wiring, ~35 new files, zero business logic).

## Risks

- `PromotionDbContext` currently has zero aggregate `DbSet<T>` — a `dotnet ef database update`/container start today would only create Outbox/Inbox tables; this is expected and matches the phase's own scope, not a defect.
- The "Contracts"/"Grpc"/"Search"/"BackgroundJobs" folders from the phase brief were resolved to their nearest existing platform equivalents (`BuildingBlock.Contract/Events/Promotion/`, `Infrastructure/GrpcClients/`+`API/GrpcServices/`, `Persistence/Search/`, `Infrastructure/BackgroundJobs/`) rather than inventing new folder names — if the architect's design uses different terminology, reconcile in Phase 2/4/6 rather than treating this placement as fixed.
