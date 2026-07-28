# Progress — 2026-07-28

Status legend: `[ ]` not started · `[~]` in progress · `[b]` blocked · `[x]` done

Source: planning-only request — "User Service Search, Elasticsearch, Localization & Cache Layer." Full read-only impact analysis performed across Product's Elasticsearch implementation, User service's current state, cache infrastructure, gRPC consumers, and (in the sibling repo) frontend screens/locale handling. See [00-architecture-and-plan.md](./00-architecture-and-plan.md) for architecture notes, dependency graph, implementation order, and risks. Tasks 1-5 (Phase A: name model, Phase B: locale/DisplayName) implemented 2026-07-28, same session as the planning pass. Tasks 6-18 remain not started.

- [x] Task 1 — Add MiddleName: Domain + Persistence (`Task1_middlename-domain-and-persistence.md`) — done: entity, EF config, migration, seeder.
- [x] Task 2 — Add MiddleName: Application layer, User service (`Task2_middlename-application-layer.md`) — done: Commands/Validators/Queries/Criteria/endpoints.
- [x] Task 3 — Propagate MiddleName: cross-service contracts, Auth + events + proto (`Task3_middlename-cross-service-contracts.md`) — done: proto, integration events, full Auth Register chain.
- [x] Task 4 — Build `ICurrentLocaleService` (`Task4_current-locale-service.md`) — done: HeaderKeys.Locale (reuses Accept-Language), interface + Infrastructure impl, wired into User's DI.
- [x] Task 5 — Build locale-aware DisplayName formatter (`Task5_displayname-formatter.md`) — done: IUserDisplayNameFormatter, wired into GetUser/GetUserDetail/SearchUsers.
- [ ] Task 6 — Scaffold User Elasticsearch search, mirror Product architecture (`Task6_elasticsearch-scaffolding.md`)
- [ ] Task 7 — Design UserSearchDocument + accent-insensitive mapping (`Task7_search-document-and-accent-insensitive-mapping.md`) — depends on Task 6; highest-uncertainty item in the epic, no in-repo precedent
- [ ] Task 8 — ProjectionBuilder + sync events, self-consumption (`Task8_projection-builder-and-sync-events.md`) — depends on Tasks 6, 7; also adds a missing `UserProfileUpdatedIntegrationEvent`
- [ ] Task 9 — RebuildUserSearchIndex command + ES config/docker wiring (`Task9_rebuild-command-and-es-config.md`) — depends on Tasks 6, 8
- [ ] Task 10 — Cut SearchUsers over to Elasticsearch-backed query (`Task10_cutover-searchusers-to-elasticsearch.md`) — depends on Tasks 8, 9
- [ ] Task 11 — User Detail cache: CacheKeys + decorator scaffold (`Task11_user-detail-cache-scaffold.md`)
- [ ] Task 12 — Wire cache invalidation into Create/Update/Delete (`Task12_cache-invalidation-wiring.md`) — depends on Task 11
- [ ] Task 13 — Extend `user.proto` with GetUser/GetUsers RPCs (`Task13_grpc-proto-getuser-getusers.md`)
- [ ] Task 14 — Implement server-side GetUser/GetUsers, cache-backed (`Task14_grpc-server-implementation.md`) — depends on Tasks 11, 13
- [ ] Task 15 — First real gRPC consumer — **needs a product decision first** (`Task15_first-grpc-consumer.md`) — depends on Task 14
- [ ] Task 16 — Migration/reindex review (`Task16_migration-and-reindex-review.md`) — gates go-live, depends on all above
- [ ] Task 17 — Testing, threaded through all phases (`Task17_testing.md`)
- [ ] Task 18 — Documentation updates (`Task18_documentation-updates.md`)

## Verification notes (Tasks 1-5)

`dotnet build` on `User.API` and `Auth.API` succeeds cleanly. A full-solution build surfaces one pre-existing, unrelated failure in `tests/unit/Order.Application.Tests/CancelOrderHandlerTests.cs` (references a removed `Order.CustomerId` — confirmed via `git status` that this test file was not touched this session, so it's a pre-existing gap, not a regression introduced here). EF migration `20260728030503_AddUserProfileMiddleName` generated via `dotnet ef migrations add`, additive and reversible. Application-level unit/integration tests for the new formatter/locale service are not yet written — tracked under Task 17.

## Key findings that reshaped the original request's assumptions

1. **gRPC "optimization" (Tasks 13-15) is greenfield, not a fix** — `user.proto` has exactly one RPC today (`CreateUserProfile`, write-only), zero services currently consume User via gRPC reads. Order/Audit both denormalize a name snapshot instead.
2. **The frontend already sends `Accept-Language` on every request** (`SimpleShopUI/src/shared/lib/api/client.ts:23`) — Task 4 needs zero frontend work to get a header flowing; only a backend reader is missing.
3. **Product's Elasticsearch mapping has no accent-folding** — only case-insensitivity via ES's default analyzer. Task 7 (accent-insensitive User search) has no in-repo precedent to copy; budget a spike.

## Cross-repo pairing

SimpleShopUI's `docs/tasks/2026-07-28/` folder has 7 frontend tasks (F1-F7), each cross-referencing the backend task it pairs with. Frontend Tasks 1-3 (forms/types) are blocked on backend Tasks 2/3/5; Frontend Task 4 (search UI) is blocked on backend Task 10; Frontend Task 5 (locale switcher UI) is explicitly optional/out-of-scope for this epic to function.
