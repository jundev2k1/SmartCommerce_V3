# Phase 1 — Bootstrap Service

## Purpose

Stand up the empty 5-layer Clean Architecture skeleton for `SmartEcommerce.PromotionService` and wire it into the solution and every piece of cross-cutting infrastructure a new service must register with — no domain entities, no CQRS features, no business logic. Follow [../../workflows/new-service-scaffold.md](../../workflows/new-service-scaffold.md) exactly; Auth Service remains the file-shape model to mirror.

## Expected Output

- Five projects under `src/Services/Promotion/`: `Promotion.Domain`, `Promotion.Application`, `Promotion.Infrastructure`, `Promotion.Persistence`, `Promotion.API`, referencing `BuildingBlock.*` per the dependency-direction rules in [../../02-architecture-rules.md](../../02-architecture-rules.md).
- `Promotion.API/Program.cs` / `DependencyInjection.cs` / `ApplicationPipeline.cs` wired via `AddBuildingBlockWeb`/`UseBuildingBlockWeb` — no hand-rolled Swagger/CORS/Carter/exception-handler code.
- Internal ports reserved: `8080` (REST), `5002` (gRPC) — only if the architect's design calls for a gRPC surface; otherwise left unused.
- New `Promotion` solution folder in `NovaCore.sln`.
- `Promotion.API/Dockerfile`, service block added to `docker-compose.yml`/`docker-compose.override.yml`.
- `promotion_db` added to `scripts/postgres/init.sql`.
- `PROMOTION_*` block added to `.env`/`.env.template`.
- `Gateway:Services:Promotion` entry added to `src/ApiGateways/YarpApiGateway/appsettings.json`.

## Build Verification

- `dotnet build` succeeds for all 5 new projects individually and as part of the full solution build.
- Service starts under `docker-compose up` and responds on its health-check endpoint.

## Completion Criteria

- Skeleton builds and runs with zero entities, zero endpoints beyond the framework health check.
- Every item on [../../workflows/new-service-scaffold.md](../../workflows/new-service-scaffold.md)'s checklist is satisfied.

## Blocked Items

- `Promotion.Persistence` has no `DbContext`/migration yet — it exists as an empty project until Phase 2/3 supply entities to map.
- No real endpoints exist yet — that starts in Phase 5.

## Dependencies

Phase 0 (Planning Freeze) only. This is the first implementation phase.
