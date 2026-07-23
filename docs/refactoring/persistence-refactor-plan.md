# Persistence Architecture Refactor — Living Plan

**Scope:** the framework-level migration introducing a Read/Write persistence-service layer across all 7 services. This is the *tracker* — status, phase-by-phase detail, risks, and the decisions made along the way. The resulting **binding standard** for how to shape persistence going forward lives in [conventions/persistence-coding-conventions.md](../conventions/persistence-coding-conventions.md) (written once the pattern is proven, end of this migration) — read that doc, not this one, when building service #8. This doc stops being actively maintained once Phase 8 closes; it remains as historical record, same convention as [08-migration-plan.md](../08-migration-plan.md).

## Why

Every service already splits repositories correctly by aggregate lifecycle (owned children like `ProductVariation`/`OrderItem` stay merged with their aggregate root's repo; independent roots like `ProductCategory`/`OrderProductCatalog` already have their own repo), and `BuildingBlock.Persistence` is already ORM/Domain-free. What's missing:

- Application handlers inject repository interfaces directly (`IProductRepository`, etc.) — there's no seam between "what the Application layer can ask for" and "how EF/Mongo actually answers it."
- 4 services' repository interfaces leak an EF-shaped `Func<IQueryable<T>, IQueryable<T>> includes` parameter into Application, meaning callers write `q => q.Include(x => x.Y)` lambdas even though `Include(` itself only executes inside Persistence.
- There's no seam for a future Read Replica / Primary split — introducing one today would mean touching every Application handler, not just Persistence.

This migration introduces explicit `I<Aggregate>ReadService`/`I<Aggregate>WriteService` pairs per aggregate root, demotes repositories to Persistence-internal implementation details, and closes the `IQueryable` leak — becoming the permanent persistence standard for every future microservice on this platform.

## Current Architecture (as of 2026-07-23, pre-migration)

- Every `<Service>.Persistence` project is flat: one `Repository/`(or `Repositories/`) folder holding one repo class per aggregate root, one `<Service>DbContext`/`MongoContext` at the project root, plus `UnitOfWork/`, `Configs/`, `Inbox/`, `Outbox/`, `Migrations/`(EF only), `Seeders/`(EF only).
- `I<Aggregate>Repository` interfaces live in `<Service>.Application/Abstractions/Repositories/` — Application injects them directly into MediatR handlers, alongside `BuildingBlock.Application.Abstractions.Persistence.IUnitOfWork` (`SaveChangesAsync`, `ExecuteTransactionAsync`).
- No `IReadService`/`IWriteService` split exists anywhere in the codebase today.
- `BuildingBlock.Persistence` is a pure contracts package (zero `PackageReference`, one `ProjectReference` to `BuildingBlock.Application`) — confirmed genuinely ORM/Domain-free.
- `BuildingBlock.Persistence.Ef` has `DbContextBase`, `EfUnitOfWork<TDbContext>` (the only place `DbUpdateConcurrencyException`/Postgres `23505` → `ConflictException` translation happens, in `ExecuteTransactionAsync`), and an **empty, uninherited stub class** `Repository/RepositoryBase.cs` — dead code, not part of any current pattern.
- `BuildingBlock.Persistence.Repository.IRepository<T>` is a generic marker interface some (not all) concrete repos implement, purely so Scrutor (`AddScopedByInterface`) can auto-register them in DI in 4 of 7 services (Auth, Inventory, Order, User). It was never visible to Application (Application doesn't reference `BuildingBlock.Persistence`).
- EF leakage into Application is otherwise minimal: zero direct `DbContext`/`Include(`/`ThenInclude(`/`AsNoTracking(` usage found outside Persistence projects anywhere in the solution. The one real leak is the `Func<IQueryable<T>, IQueryable<T>> includes` parameter on:
  - `Inventory.Application/Abstractions/Repositories/{IWarehouseRepository,IInventoryRepository}.cs`
  - `User.Application/Abstractions/Repositories/IUserRepository.cs`
  - `Auth.Application/Abstractions/Repositories/{IAccountRepository,IRefreshTokenRepository}.cs`
  - `Order.Application/Abstractions/Repositories/{IOrderProductCatalogRepository,IOrderRepository}.cs`
- Cart has **no persisted aggregate** — it's Redis-only, entirely inside the Order service, via `Order.Application/Abstractions/Services/ICartService.cs` (Application-owned port) → `Order.Infrastructure/Caching/CartService.cs` (implementation, using the generic `ICacheService` port). No repository, no DbContext, no EF/Mongo involvement — **out of scope for this migration**, already correctly abstracted.

## Target Architecture

Per aggregate root, inside `<Service>.Persistence/`, using the plural form of the aggregate name (see Naming below):

```
<Service>.Persistence/
├── <Aggregates>/
│   ├── Read/<Aggregate>ReadService.cs      — implements I<Aggregate>ReadService; owns Include/ThenInclude/AsNoTracking/projection/pagination
│   ├── Write/<Aggregate>WriteService.cs    — implements I<Aggregate>WriteService; owns load-modify-save workflow + commit
│   └── Repositories/
│       ├── I<Aggregate>Repository.cs       — Persistence-internal now (moved out of Application)
│       └── <Aggregate>Repo.cs              — stays thin: Add/Remove/trivial no-include lookups only
├── Configs/, Outbox/, Inbox/, Migrations/, Seeders/, Saga/   — stay flat, cross-cutting, untouched by this migration
├── <Service>DbContext.cs                   — stays at project root, single shared context; both Read/Write inject it today (no replica exists yet), but the seam to add one later is now Persistence-only
└── DependencyInjection.cs
```

Single-aggregate services (User, Audit) collapse to one folder — don't pre-build structure for aggregates that don't exist.

**Interface ownership.** `I<Aggregate>ReadService`/`I<Aggregate>WriteService` live in `<Service>.Application/Abstractions/Persistence/<Aggregate>/` — Application owns the port, same pattern `IUnitOfWork` already follows. `I<Aggregate>Repository` moves out of Application entirely; Application never sees a repository interface again once a service is migrated. MediatR handlers swap `I<Aggregate>Repository` → `I<Aggregate>ReadService` (queries) or `I<Aggregate>WriteService` (commands).

**Repository responsibility.** `Add`, `Remove`, trivial no-include lookups only. No `Include`/projection/tracked-aggregate-loading-for-update — that's a workflow, owned by the Write Service (load-modify-save path) or Read Service (query/projection path). Both services may inject `TDbContext` directly in addition to the repo — EF is a legitimate implementation detail inside Persistence.

**Write Service commit rule.** See Architectural Decisions → Correction 1 below.

**Cross-aggregate transactions.** See Architectural Decisions → Correction 2 below.

## Architectural Decisions

These were surfaced by stress-testing the naive "Write Service always calls `SaveChangesAsync()` itself" design against the actual codebase (not assumed) before Phase 1 started, and are binding for every phase.

### Correction 1 — Write Services default to `ExecuteTransactionAsync`, not bare `SaveChangesAsync`

`EfUnitOfWork<TDbContext>.ExecuteTransactionAsync` (`src/BuildingBlocks/BuildingBlock.Persistence.Ef/UnitOfWork/EfUnitOfWork.cs`) is where `DbUpdateConcurrencyException` and Postgres `23505` unique-violation get translated to `ConflictException`, plus `ChangeTracker.Clear()` on failure. Bare `SaveChangesAsync()` is a passthrough with none of that. `ExecuteTransactionAsync` is the *dominant* commit pattern today (Product ×12, Inventory ×8, Order ×6, Auth ×3, User ×2 call sites) — e.g. `DeductStockHandler`'s bounded concurrency-retry loop only works because it catches the typed `ConflictException` that `ExecuteTransactionAsync` throws.

**Rule:** a `WriteService` method wraps its repo call in `unitOfWork.ExecuteTransactionAsync(...)` if the handler it's replacing already did so (the default, most common case). It uses bare `SaveChangesAsync()` only where the current handler already does (the `OrderProductCatalog` event-sync handlers in Order). Mongo `WriteService`s (Audit, Notification) always use `SaveChangesAsync()` — it's a documented no-op there, not a decision to revisit. This removes `IUnitOfWork` from most handler constructors, but not all — see Correction 2.

### Correction 2 — Named exception for genuine cross-aggregate-root transactions (Inventory)

`AdjustStockHandler`/`DeductStockHandler` (confirmed; `RestockStock`/`StockIn`/`StockOut` to verify in Phase 4) mutate `InventoryRepo` + `InventoryTransactionRepo` (+ `StockDeductionRepo` for DeductStock) — three independent aggregate roots per Inventory's own audit-hierarchy registration — inside **one** atomic `ExecuteTransactionAsync`. Splitting this into independent per-aggregate commits would be a real atomicity regression (a crash between commits leaves stock decremented with no audit trail / no idempotency record).

**Rule:** for these handlers only, the handler keeps injecting `IUnitOfWork` directly plus the relevant `WriteService`s. Each affected `WriteService` additionally exposes a non-committing `Stage*` method (repo mutation only, no commit), used solely by these handlers. The handler wraps multiple `Stage*` calls in one `ExecuteTransactionAsync`. This is a **permanent, documented pattern** for any future ledger/journal-shaped aggregate — not tech debt to clean up later.

### Correction 2a — batched-write extension (discovered Phase 3)

Correction 2 was written for cross-*aggregate-root* transactions (Inventory). Phase 3 found the same shape triggered by a different cause: `RefreshTokenSyncService` batches many `Add`/`Update` calls for potentially many tokens across a whole user-batch into one `IUnitOfWork.ExecuteTransactionAsync` (with its own deadlock-retry loop) — all against the *same* aggregate type, not multiple aggregate roots. The fix is identical in shape: `RefreshTokenWriteService.AddAsync`/`UpdateAsync` are non-committing (no `Stage` prefix needed since there's no separate committing counterpart to disambiguate from — staging is these methods' only shape), and the caller keeps `IUnitOfWork` injected directly. **Generalized rule:** Correction 2 applies whenever a single logical operation needs to commit more than one persistence mutation atomically — whether that's multiple aggregate roots (Inventory) or multiple instances of one aggregate batched together (Auth) — not only the narrower "different aggregate roots" case.

### Correction 1a — outbox enqueue vs. a self-committing Write Service (discovered Phase 1)

Several create/update handlers (`CreateUserHandler`, and the same shape will recur in Product/Order/Auth) enqueue an integration event via `IOutboxStore.EnqueueAsync` in the *same* transaction as the aggregate write — required for the transactional-outbox guarantee. Once a `WriteService` owns its own commit (Correction 1), the handler can no longer wrap both calls in one `ExecuteTransactionAsync` block itself. This is safe without any new API: `IOutboxStore.EnqueueAsync` (and every `EfOutboxStore`/service adapter beneath it) only stages an `Add` on the shared scoped `DbContext`'s change tracker — it never calls `SaveChangesAsync` itself. So the handler just calls `outboxStore.EnqueueAsync(...)` *before* `writeService.CreateAsync(...)`; the Write Service's internal `ExecuteTransactionAsync` → `Context.SaveChangesAsync()` persists everything currently tracked (the staged outbox row plus the entity added inside the Write Service's own lambda) in one commit, regardless of which call staged which change. Order matters (enqueue before, not after, the Write Service call) — commit-then-enqueue would split them into two transactions and break the atomicity guarantee. No interface changed to support this; it's a call-ordering rule, not a new method.

### Naming

- **Per-aggregate folder/namespace segments use the plural form** (`UserProfiles/`, `Products/`, `ProductCategories/`, `Orders/`, `Inventories/`, ...), matching each service's own existing `DbSet<T>` property name. Discovered during Phase 1: a singular segment (e.g. `namespace User.Persistence.UserProfile`) collides with the `UserProfile` entity type itself — C# resolves the nested namespace before the `using`-imported type, producing a `CS0118` ("X is a namespace but is used like a type") error the moment the aggregate type is referenced unqualified inside its own namespace. Pluralizing sidesteps this entirely and happens to match the codebase's existing `DbSet` naming, so it's adopted everywhere, not just where the collision bites.
- Keep the existing abbreviated convention: `<Aggregate>Repo` (class), `I<Aggregate>Repository` (interface) — not `<Aggregate>Repository` as a class name. `<Aggregate>ReadService`/`<Aggregate>WriteService` use full words (new concept, no prior abbreviation to match).
- `IUserRepository` → renamed to `IUserProfileRepository` while moving (fixes a pre-existing repo/interface name mismatch: the class was already `UserProfileRepo`).
- Product's `ProductCategory`/`ProductTag` keep their full existing prefix (`ProductCategoryRepo`, `IProductCategoryRepository`, folder `ProductCategory/`) — not shortened to `Category`/`Tag`, to avoid mismatching the actual Domain entity names.
- Nothing else gets renamed as a drive-by (e.g. Auth's `Config/` vs. other services' `Configs/`, `OrderProductCatalogRepo`'s existing `GetByVariantionIdsAsync` typo — both stay, out of scope).

### Deletions / no-changes

- **Deleted:** `BuildingBlock.Persistence.Ef.Repository.RepositoryBase` — empty stub, zero inheritors, not part of the target design.
- **Unchanged:** `BuildingBlock.Persistence.Repository.IRepository<T>` — stays exactly as-is, still a valid Scrutor DI-scanning marker, still invisible to Application.
- **Out of scope:** Cart (Order service) — no repository/DbContext exists; already correctly abstracted via `ICacheService`.
- **Out of scope:** `Product.Persistence.Elasticsearch` — search-index sync, not transactional persistence.
- **Out of scope:** Auth's `IAuthService`/`UserManager<Account>`-based mutation flow — a separate concern from `AccountRepo`, must not balloon into this migration.
- **Out of scope:** `Order.Persistence/Saga/` (`EfSagaStore`, `SagaExecutionRecordEntity`) — not a domain aggregate; its singleton lifetime (opens its own DI scope per call) must not be disturbed.

The `Func<IQueryable<T>, IQueryable<T>> includes` leak is fixed inline as each affected service's phase lands (replaced by named Include-composing methods on the Read/Write service) — not a separate cleanup pass.

## Migration Phases & Progress Checklist

Order: **User → Audit → Auth → Inventory → Product → Notification → Order**, then the architecture-standard doc. Rationale: prove Correction 1 on User's 8 low-risk single-aggregate handlers first; prove the (simpler) Mongo shape on Audit next; isolate Auth's layering surprise (real consumers are a gRPC service + Infrastructure background jobs, not MediatR handlers) to its own phase; hit Inventory's genuine cross-aggregate case (Correction 2) only after the simple pattern is proven; Product and Notification are highest-file-count but lowest-per-file-risk; Order is last since it's the only service combining Saga + Outbox/Inbox + an owned collection + an independent read-model.

Each phase: create new files → move/delete old files → update DI → swap handler constructors → scoped build (only that service's `.csproj` chain, per [[ai_execution_strategy]]) → run affected tests → docker-verify that one service → update `docs/services/<name>-service.md` → update the checklist below → commit (Conventional Commits).

- [ ] **Phase 0 — Docs scaffolding.** This file + `refactoring/README.md` + `docs/README.md` index update. Commit: `docs(refactoring): add persistence refactor plan`.
- [x] **Phase 1 — User** (`UserProfile`). New `UserProfiles/{Read,Write,Repositories}`. Renamed `IUserRepository`→`IUserProfileRepository` (and concrete class `UserRepository`→`UserProfileRepo`, fixing a pre-existing name mismatch). All 8 handlers (`Features/Users/**`) swapped constructors; `UserProfileRepo` dropped `IRepository<T>` (its shape no longer fits — no more `GetByIdAsync`/`UpdateAsync` on the repo), so it's now manually registered in DI instead of Scrutor-scanned. Scoped build (`User.API` chain) green. Surfaced Correction 1a (outbox-enqueue ordering) — see above.
- [x] **Phase 2 — Audit** (`AuditLogEntry`, Mongo). New `AuditLogs/{Read,Write,Repositories}`. 3 handlers swapped. Repo trimmed to `AddAsync` only (append-only aggregate); `GetByIdAsync`/`SearchAsync` moved to `AuditLogReadService`, querying `AuditMongoContext` directly (no repo delegation, matching User's precedent). `AuditLogWriteService.AddAsync` calls `unitOfWork.SaveChangesAsync()` — stays a documented no-op. Scoped build (`Audit.API` chain) green.
- [x] **Phase 3 — Auth** (`Account`, `RefreshToken`). Confirmed zero callers solution-wide for `AccountRepo`'s `GetByIdAsync` ×2, `UpdateAsync` ×2, `GetActiveUsersAsync` — dropped rather than ported; `IAccountReadService`/`IAccountWriteService` ended up genuinely thin (2 methods total: `GetByEmailAsync`, `DeleteIfExistAsync`). Real consumers found via solution-wide grep, not a `*Handler.cs` search: `OnAccountDeletionInitiatedHandler`, `Auth.API/GrpcServices/AuthGrpcServiceImpl.cs` (gRPC), and for RefreshToken, `Auth.Infrastructure/Security/RefreshTokens/Initialization/RefreshTokenInitializationService.cs` + `Auth.Infrastructure/BackgroundJobs/Jobs/RefreshTokenSync/RefreshTokenSyncService.cs`. Extended Correction 2 to `RefreshTokenWriteService.AddAsync`/`UpdateAsync` (non-committing — the sync job batches many calls into one `ExecuteTransactionAsync` it owns). `IAuthService`/`UserManager<Account>` mutation flow untouched, as scoped. Scoped build (`Auth.API` chain) green.
- [x] **Phase 4 — Inventory** (`Inventory`, `Warehouse`, `InventoryTransaction`, `StockDeduction`). Verified all 14 handlers individually via solution-wide grep of every repo call site — confirmed **all 5** stock-mutation handlers (`AdjustStock`, `DeductStock`, `RestockStock`, `StockIn`, `StockOut`) are cross-aggregate (Correction 2 applies to every one, not just Adjust/Deduct as originally guessed); `CreateWarehouse` + the 3 `OnProduct*` event handlers are single-aggregate, normal self-committing shape. `CreateWarehouse` commits via bare `SaveChangesAsync` (not `ExecuteTransactionAsync`), preserved as-is per Correction 1. Every `Func<IQueryable<T>,...>` includes-overload and several other repo methods (`GetByIdAsync(includes)`, `UpdateAsync(includes)`, `IInventoryRepository.GetByVariationIdAsync`, `.DeleteAsync`, all `IWarehouseRepository.UpdateAsync` overloads) had zero callers solution-wide — dropped rather than ported. `InventoryReadService` absorbed the rollup methods (`GetTotalStockBy*`) off the repo. `WarehouseRepo` (the one repo still Scrutor-scanned via `IRepository<T>` pre-migration) dropped it along with the other three once trimmed. Scoped build (`Inventory.API` chain) green on first attempt.
- [ ] **Phase 5 — Product** (`Product`+`ProductVariation`, `ProductCategory`, `ProductTag`). No cross-aggregate write transactions confirmed. Verify Elasticsearch self-consuming-inbox sync still works post-phase (not just that Postgres builds/tests pass).
- [ ] **Phase 6 — Notification** (7 aggregates). Mechanically identical to Audit ×7 — no `ExecuteTransactionAsync` callers in this service at all.
- [ ] **Phase 7 — Order** (`Order`+`OrderItem`, `OrderProductCatalog`). Independent Read/Write pairs for both, confirmed write-independent today. `Saga/` stays flat and untouched. `Features/Cart/**` stays out of scope.
- [ ] **Phase 8 — Architecture standard.** Write `docs/conventions/persistence-coding-conventions.md`, update `docs/02-architecture-rules.md`, `docs/03-building-blocks-reference.md`, `docs/workflows/add-new-repository.md` (rewrite for the new pattern), `docs/06-implementation-templates.md`'s repository template. Close out this checklist.

## Risk Register

| Risk | Phase | Mitigation |
|---|---|---|
| `ConflictException` translation silently lost | All EF phases | Correction 1 applied as a hard rule from Phase 1 onward |
| Cross-aggregate stock transaction split into non-atomic commits | 4 (Inventory) | Correction 2's `Stage*`-method exception; verify all 5 stock handlers + 3 event handlers individually |
| Self-consuming Product→Elasticsearch inbox desync | 5 (Product) | Don't touch `Product.Persistence.Elasticsearch`; verify ES sync post-phase |
| Auth's non-MediatR consumers missed | 3 (Auth) | Explicitly check `Auth.API/GrpcServices/` and `Auth.Infrastructure/{Security,BackgroundJobs}/`, not just `Features/**Handler.cs` |
| `EfSagaStore` singleton lifetime broken | 7 (Order) | `Saga/` stays flat, no scoped dependency added to it |
| Dead repo methods ported forward as unused code | 3 (Auth) | Confirm zero callers for `AccountRepo`'s unused-looking methods before porting |
| Docs drift from code | All phases | Update the relevant `docs/services/*.md` in the same commit as each phase, not deferred |

## Open Questions

- Auth's dead-code-looking methods on `IAccountRepository` (`GetByIdAsync`, `UpdateAsync`, `GetActiveUsersAsync`): default = drop if zero callers confirmed during Phase 3.
- `OrderProductCatalogRepo`'s existing `GetByVariantionIdsAsync` typo: default = leave as-is, out of scope for this migration.
- ~~`RestockStock`/`StockIn`/`StockOut` transaction shape: unverified, confirm during Phase 4~~ — resolved: all three are cross-aggregate, same as Adjust/Deduct (Correction 2 applies).
