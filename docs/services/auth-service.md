# Auth Service

**Scope:** Auth-specific facts — routes, config, folder layout, known state. This is **the reference implementation** — when a general pattern doc says "see the reference implementation," it means here. General patterns themselves live in [04-coding-rules.md](../04-coding-rules.md)/[02-architecture-rules.md](../02-architecture-rules.md), not repeated here.

## Projects

`Auth.Domain`, `Auth.Application`, `Auth.Infrastructure`, `Auth.Persistence`, `Auth.API` — standard 5-layer split, see [02-architecture-rules.md](../02-architecture-rules.md#layer-responsibilities).

## Ports & routing

- Internal: `8080` (REST), `5002` (gRPC client target for User Service). Not published to host directly — only reachable through the Gateway.
- Gateway path prefix: `/api/auth/` (`RequireAuth: false` — Auth's own endpoints are anonymous; they issue the tokens other services require).

## Routes (Carter endpoints, `Auth.API/Endpoints/`)

| Method | Route | File | Purpose |
|---|---|---|---|
| POST | `/register` | `Register.cs` | Create account, triggers `OnUserRegisteredEvent` → gRPC `CreateUserProfile` on User Service |
| POST | `/login` | `Login.cs` | Issue AccessToken/RefreshToken cookies |
| POST | `/logout` | `Logout.cs` | Revoke refresh token, clear cookies |
| POST | `/refresh-token` | `RefreshToken.cs` | Reads `RefreshToken` cookie, validates against Redis, issues new tokens. **Also filtered at the Gateway** — see [gateway.md](gateway.md) |

## DI composition (`Auth.API/DependencyInjection.cs`, `Auth.API/ApplicationPipeline.cs`)

```csharp
// Program.cs
builder.Services.AddPersistence(config).AddApplication().AddInfrastructure(config).AddPresentation(config);
// AddPresentation → AddBuildingBlockWeb(config, WebOptions) + AddCommonAuthorizationPolicies()
// UseApplication → SeedDatabase, InitializeRefreshTokenCache, UseBackgroundJobsDashboard/Scheduling, UseBuildingBlockWeb, MapEndpoints
```

`Auth.Infrastructure/DependencyInjection.cs` chains: `AddAppLogger → AddRedisCache → AddRoleCaching → AddBackgroundJobs → AddInboxOutboxCleanupJobs → AddSecurityServices → AddApplicationEventDispatcher → AddKafkaMessaging("auth-service") → AddGrpcClients`. (`AddApplicationEventDispatcher` registers `IInternalEventDispatcher` — legacy method name, see [reference/events.md](../reference/events.md#the-two-tiers).) `AddRoleCaching` decorates `IAuthService` with `CachedAuthServiceDecorator` — this is why `AddPersistence` must run before `AddInfrastructure` (see [02-architecture-rules.md](../02-architecture-rules.md#composition-root-convention-per-service)).

## Auth-specific building blocks (not shared with User)

- **Hangfire recurring job** (`Auth.Infrastructure/BackgroundJobs/Jobs/RefreshTokenSync/`) — `RefreshTokenSyncService : IRecurringJob`, dashboard at `/hangfire`. The Hangfire bootstrap itself (`AddHangfireScheduling`, recurring-job discovery, dashboard) now lives in `BuildingBlock.Infrastructure/BackgroundJobs/` and is shared with User (see [user-service.md](user-service.md)) — Auth's `BackgroundJobsExtensions` is a thin wrapper that only supplies its own job-assembly marker and dashboard title. See [workflows/add-background-job.md](../workflows/add-background-job.md).
- **Inbox/Outbox cleanup jobs** — shared `OutboxCleanupJob`/`InboxCleanupJob` (`BuildingBlock.Infrastructure/BackgroundJobs/Cleanup/`), registered via `.AddInboxOutboxCleanupJobs(configuration)`. See [reference/inbox-outbox-runtime.md](../reference/inbox-outbox-runtime.md#cleanup).
- **JWT issuance** — `Auth.Infrastructure/Security/Jwt/JwtTokenGenerator.cs` (`IJwtTokenGenerator`) creates the tokens; `Auth.Infrastructure/Security/RefreshTokens/RefreshTokenService.cs` manages refresh-token lifecycle in Redis (key: `refresh_token_by_string:{token}`, this is the format the Gateway's filter middleware also reads — see [gateway.md](gateway.md)). Token *validation* middleware (`AddJwtBearerAuthentication`) is shared via `BuildingBlock.Web`, same as every other service.
- **gRPC client** — `Auth.Infrastructure/GrpcClients/UserProfileServiceClient.cs` calls User Service's `CreateUserProfile` after registration.
- **Role caching** — `RoleCacheService` + `CachedAuthServiceDecorator`, full pattern in [reference/caching.md](../reference/caching.md).

## Persistence: Read/Write services, and where Identity fits

Phase 3 of the [persistence Read/Write migration](../refactoring/persistence-refactor-plan.md). `Auth.Application/Abstractions/Persistence/{Accounts,RefreshTokens}/` hold `IAccountReadService`/`IAccountWriteService` and `IRefreshTokenReadService`/`IRefreshTokenWriteService` — the only persistence ports Application/Infrastructure inject now; the old `IAccountRepository`/`IRefreshTokenRepository` are gone from Application entirely. Two things distinguish Auth from a typical phase:

- **`AccountReadService`/`AccountWriteService` are intentionally thin.** Tracing every real caller solidly ahead of writing them showed `IAccountRepository` had exactly two live consumers — `OnAccountDeletionInitiatedHandler` (`DeleteIfExistAsync`) and `Auth.API/GrpcServices/AuthGrpcServiceImpl.cs` (`GetByEmailAsync`, a gRPC service, not a MediatR handler — easy to miss with a `*Handler.cs`-only search). Everything else on the old interface (`GetByIdAsync` ×2, `UpdateAsync` ×2, `GetActiveUsersAsync`) had zero callers anywhere in the solution and was dropped rather than carried forward. Actual Account *mutation* (register, update, delete-for-real) goes through `IAuthService`/ASP.NET Identity's `UserManager<Account>` (auto-saves internally), which stayed completely out of this migration's scope — it's a separate concern from the repository.
- **`RefreshTokenWriteService.AddAsync`/`UpdateAsync` are deliberately non-committing.** Their only caller, `RefreshTokenSyncService` (`Auth.Infrastructure/BackgroundJobs/Jobs/RefreshTokenSync/`), batches many adds/updates for a whole user-batch inside one `IUnitOfWork.ExecuteTransactionAsync` it owns itself (with its own deadlock-retry loop) — so unlike every other Write Service in this migration, these two methods just stage the mutation and let the caller decide the commit boundary. This is the same "named exception" shape as Inventory's cross-aggregate stock transactions (Correction 2 in the tracker), just triggered by batching within one aggregate type instead of across aggregate roots. `RefreshTokenReadService.GetByUserIdAsync` is a normal read, used by `RefreshTokenInitializationService` at startup.

## Known state

- Mapster is registered but unused — hand-mapping is the actual convention (see [04-coding-rules.md](../04-coding-rules.md#mapping)).
- `Register` → identity persistence goes through ASP.NET Identity's `UserManager` (auto-saves); it does **not** call `IUnitOfWork.SaveChangesAsync` explicitly. The `ExecuteTransactionAsync` pattern (see [04-coding-rules.md#transaction-management](../04-coding-rules.md#transaction-management)) is demonstrated in `RefreshTokenSyncService.SyncCacheToDbAsync`, not in Register/Login.
- gRPC to User Service is config-gated (`Grpc:UserService:Url`, with a stub fallback historically used during local dev without User Service running) — check current `Auth.Infrastructure/GrpcClients/DependencyInjection.cs` wiring before assuming it's always live.
