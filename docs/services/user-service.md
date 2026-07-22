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

## Documented divergence from Auth: repository style

User injects the generic `IRepository<UserProfile>` (`BuildingBlock.Application.Abstractions.Persistence.IRepository<T>`) **directly** in handlers — it does not define a specific `IUserProfileRepository` the way Auth defines `IAccountRepository`. This is an **accepted divergence**, not drift: User's queries so far don't need anything beyond generic CRUD. If a future User feature needs a query the generic interface can't express, add a specific interface following Auth's pattern (see [04-coding-rules.md](../04-coding-rules.md#repository--unit-of-work)) rather than bolting extra methods onto handlers.

## Known issues

- **`GetUserQueryHandler` and `UpdateUserCommandHandler` throw raw `InvalidOperationException`** for not-found cases instead of `BuildingBlock.Application.Exceptions.NotFoundException`. This violates the exception rule in [02-architecture-rules.md](../02-architecture-rules.md#exception-rule) — `ExceptionHandlerHelper` doesn't recognize `InvalidOperationException`, so these paths likely surface as a masked 500 instead of a 404. The sibling `GetUserDetailQueryHandler` in the same folder does it correctly (throws `UnauthorizedException`/`NotFoundException`). **This is a real bug, not just a doc gap** — flag for a fix; do not copy the `InvalidOperationException` pattern into new handlers.
- Mapster registered but unused, same as Auth — see [04-coding-rules.md](../04-coding-rules.md#mapping).
