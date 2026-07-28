# Progress — 2026-07-28

Status legend: `[ ]` not started · `[~]` in progress · `[b]` blocked · `[x]` done

Source: planning-only request — "User Service Search, Elasticsearch, Localization & Cache Layer." Full read-only impact analysis performed across Product's Elasticsearch implementation, User service's current state, cache infrastructure, gRPC consumers, and (in the sibling repo) frontend screens/locale handling. See [00-architecture-and-plan.md](./00-architecture-and-plan.md) for architecture notes, dependency graph, implementation order, and risks. Tasks 1-10 (Phase A: name model, Phase B: locale/DisplayName, Phase D: Elasticsearch search) implemented 2026-07-28, same session as the planning pass. Tasks 11-18 (cache, gRPC, migration review, testing, docs) remain not started.

- [x] Task 1 — Add MiddleName: Domain + Persistence (`Task1_middlename-domain-and-persistence.md`) — done: entity, EF config, migration, seeder.
- [x] Task 2 — Add MiddleName: Application layer, User service (`Task2_middlename-application-layer.md`) — done: Commands/Validators/Queries/Criteria/endpoints.
- [x] Task 3 — Propagate MiddleName: cross-service contracts, Auth + events + proto (`Task3_middlename-cross-service-contracts.md`) — done: proto, integration events, full Auth Register chain.
- [x] Task 4 — Build `ICurrentLocaleService` (`Task4_current-locale-service.md`) — done: HeaderKeys.Locale (reuses Accept-Language), interface + Infrastructure impl, wired into User's DI.
- [x] Task 5 — Build locale-aware DisplayName formatter (`Task5_displayname-formatter.md`) — done: IUserDisplayNameFormatter, wired into GetUser/GetUserDetail/SearchUsers.
- [x] Task 6 — Scaffold User Elasticsearch search, mirror Product architecture (`Task6_elasticsearch-scaffolding.md`) — done.
- [x] Task 7 — Design UserSearchDocument + accent-insensitive mapping (`Task7_search-document-and-accent-insensitive-mapping.md`) — done via an additive BuildingBlock.Search overload; live-ES verification of diacritic handling still open (Task 17).
- [x] Task 8 — ProjectionBuilder + sync events, self-consumption (`Task8_projection-builder-and-sync-events.md`) — done, including the previously-missing `UserProfileUpdatedIntegrationEvent`; two triggers dispatch inline rather than via self-consumption (documented deviation).
- [x] Task 9 — RebuildUserSearchIndex command + ES config/docker wiring (`Task9_rebuild-command-and-es-config.md`) — done.
- [x] Task 10 — Cut SearchUsers over to Elasticsearch-backed query (`Task10_cutover-searchusers-to-elasticsearch.md`) — done: full cutover; live E2E run and parity checks still open (Tasks 16/17).
- [ ] Task 11 — User Detail cache: CacheKeys + decorator scaffold (`Task11_user-detail-cache-scaffold.md`)
- [ ] Task 12 — Wire cache invalidation into Create/Update/Delete (`Task12_cache-invalidation-wiring.md`) — depends on Task 11
- [ ] Task 13 — Extend `user.proto` with GetUser/GetUsers RPCs (`Task13_grpc-proto-getuser-getusers.md`)
- [ ] Task 14 — Implement server-side GetUser/GetUsers, cache-backed (`Task14_grpc-server-implementation.md`) — depends on Tasks 11, 13
- [ ] Task 15 — First real gRPC consumer — **needs a product decision first** (`Task15_first-grpc-consumer.md`) — depends on Task 14
- [ ] Task 16 — Migration/reindex review (`Task16_migration-and-reindex-review.md`) — gates go-live, depends on all above
- [ ] Task 17 — Testing, threaded through all phases (`Task17_testing.md`)
- [ ] Task 18 — Documentation updates (`Task18_documentation-updates.md`)

## Verification notes (Tasks 1-10)

`dotnet build` on `User.API`, `Auth.API`, and `Product.API` (sanity check for the `BuildingBlock.Search` change) all succeed cleanly, as does a full-solution build except one pre-existing, unrelated failure in `tests/unit/Order.Application.Tests/CancelOrderHandlerTests.cs` (references a removed `Order.CustomerId` — confirmed via `git status` that this test file was not touched this session). EF migration `20260728030503_AddUserProfileMiddleName` generated via `dotnet ef migrations add`, additive and reversible. No Docker/Elasticsearch stack was started this session, so live-ES behavior (accent-folding, real query results, index rebuild against real data) is unverified — only compile-time correctness is confirmed. Application-level unit/integration tests for the new formatter/locale service/search behavior are not yet written — tracked under Task 17.

## Key findings that reshaped the original request's assumptions

1. **gRPC "optimization" (Tasks 13-15) is greenfield, not a fix** — `user.proto` has exactly one RPC today (`CreateUserProfile`, write-only), zero services currently consume User via gRPC reads. Order/Audit both denormalize a name snapshot instead.
2. **The frontend already sends `Accept-Language` on every request** (`SimpleShopUI/src/shared/lib/api/client.ts:23`) — Task 4 needs zero frontend work to get a header flowing; only a backend reader is missing.
3. **Product's Elasticsearch mapping has no accent-folding** — only case-insensitivity via ES's default analyzer. Task 7 (accent-insensitive User search) has no in-repo precedent to copy; budget a spike.

## Cross-repo pairing

SimpleShopUI's `docs/tasks/2026-07-28/` folder has 7 frontend tasks (F1-F7), each cross-referencing the backend task it pairs with. Frontend Tasks 1-3 (forms/types) are blocked on backend Tasks 2/3/5; Frontend Task 4 (search UI) is blocked on backend Task 10; Frontend Task 5 (locale switcher UI) is explicitly optional/out-of-scope for this epic to function.
