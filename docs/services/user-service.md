# User Service

**Scope:** User-specific facts and its documented divergences from the [Auth Service](auth-service.md) reference implementation. General patterns live in [04-coding-rules.md](../04-coding-rules.md)/[02-architecture-rules.md](../02-architecture-rules.md) — not repeated here.

## Projects

`User.Domain`, `User.Application`, `User.Infrastructure`, `User.Persistence`, `User.API` — same 5-layer split as Auth.

## Ports & routing

Internal `8080` (REST) / `5002` (gRPC server — User is the gRPC *server* here, Auth is the client). Gateway path prefix `/api/user/` (`RequireAuth: false`).

## Routes (Carter endpoints, `User.API/Endpoints/`)

| Method | Route | File | Purpose |
|---|---|---|---|
| POST | `/profiles` | `CreateUser.cs` | Create a user profile (also invoked via gRPC from Auth's registration flow) |
| GET | `/profiles/{userId}` | `GetUser.cs` | Fetch a profile by id |
| GET | `/profiles/current/detail` | `GetUserDetail.cs` | Fetch the current authenticated user's detail |
| PUT | `/profiles/{userId}` | `UpdateUser.cs` | Update a profile |

## User-specific building blocks (not present in Auth)

- **gRPC server** — `User.API/GrpcServices/UserGrpcServiceImpl.cs` (`UserGrpcService.UserGrpcServiceBase`) exposes `CreateUserProfile`; thin adapter, dispatches `CreateUserCommand` via `ISender`, no business logic.
- **Hangfire background jobs** (`User.Infrastructure/BackgroundJobs/BackgroundJobsExtensions.cs`) — User has its own Hangfire storage (`ConnectionStrings:Hangfire` → `user_hangfire_db`) and dashboard at `/hangfire`, wired through the shared `BuildingBlock.Infrastructure` bootstrap (`AddHangfireScheduling`/`AddInboxOutboxCleanupJobs`, same as Auth — see [auth-service.md](auth-service.md#auth-specific-building-blocks-not-shared-with-user)). Currently the only recurring jobs registered are the shared Inbox/Outbox cleanup jobs; User has no service-specific `IRecurringJob` of its own yet.
- **Integration event consumer** — `User.Infrastructure/Messaging/Consumers/UserAccountDeletionIntegrationEventConsumer.cs` (`IIntegrationEventConsumer`, topic `user-service.useraccountdeletionintegrationevent`) dispatches `DeleteUserProfileCommand`. This is the inbound half of the account-deletion flow documented in [reference/events.md](../reference/events.md).
- **Idempotency handling** — `CreateUserCommandHandler` does check-then-create, then on `SaveChangesAsync` failure re-checks for a concurrent winning write. Necessary because it can be invoked twice for the same user (gRPC call + potential Kafka redelivery). Auth's `RegisterHandler` doesn't need this — registration is a single client-initiated call, not idempotency-sensitive the same way. **If you're writing a handler that can be triggered by both a direct call and an event/gRPC path, follow this pattern, not Register's.**
- **Read-only role cache consumer** — `User.Infrastructure/Caching/RoleCacheReader.cs` (`IRoleCacheReader`) reads the role cache that *Auth* owns and writes (`RoleCacheService`). See [reference/caching.md](../reference/caching.md#cross-service-read-only-consumer).

## Persistence: Read/Write services

User was the reference implementation (Phase 1) for the [persistence Read/Write migration](../refactoring/persistence-refactor-plan.md) — see [conventions/persistence-coding-conventions.md](../conventions/persistence-coding-conventions.md) for the binding standard. Handlers inject `IUserProfileReadService`/`IUserProfileWriteService` (`User.Application/Abstractions/Persistence/UserProfiles/`), never a repository interface. `UserProfileRepo` (`User.Persistence/UserProfiles/Repositories/`) implements the generic `BuildingBlock.Persistence.Repository.IRepository<UserProfile>` in full plus the specific `IUserProfileRepository` (one real member: `DeleteWithNoTrackingAsync`, a bulk `ExecuteDeleteAsync` that doesn't fit the generic shape) — `UserProfileWriteService` depends on the generic interface for standard mutations. Per the tracker's Course Correction: `CreateUserHandler`/`UpdateUserHandler` own `IUnitOfWork.ExecuteTransactionAsync` themselves and call `IUserProfileWriteService.CreateAsync`/`UpdateProfileDetailsAsync` (intent-named, not a delegate), which just stage the repo mutation; `DeleteUserHandler`/`OnUserInitiatedHandler`'s methods commit via bare `SaveChangesAsync` inside the `WriteService` itself, matching each handler's pre-migration commit shape exactly.

## Known issues

- Mapster registered but unused, same as Auth — see [04-coding-rules.md](../04-coding-rules.md#mapping).
