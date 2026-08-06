# Execution Context

**Scope:** the ambient, read-only snapshot of the current request's identity - who's calling, which
Tenant/Scope they belong to, and the request's correlation/idempotency/locale metadata. Lives in
`BuildingBlock.SharedKernel` (the type itself - zero dependencies, so every layer of the platform
can see it), initialized exclusively by `BuildingBlock.Web`'s `ExecutionContextMiddleware`.

## Why it exists

Framework components that run underneath a request - EF Core interceptors and model-building
conventions, Mapster profiles, Outbox/Inbox, Audit - need to know the current user/tenant/scope,
but none of them can (or should) take a DI dependency on `HttpContext`. Before this existed, that
need leaked into places it didn't belong: `DbContextBase` resolved `ICurrentTenantService` via
`this.GetService<T>()` inside `OnModelCreating`, meaning a persistence component had an opinion
about *where* request identity came from. `ExecutionContext` replaces that with a single ambient
accessor any component can read without DI at all:

```csharp
NovaCore.BuildingBlock.SharedKernel.Context.ExecutionContext.Current.TenantId
```

## Why it is not a DbContext

`ExecutionContext` has nothing to do with EF Core. It doesn't track entities, doesn't open
connections, doesn't participate in `SaveChanges`. It is a plain, framework-agnostic snapshot of
identity - the persistence layer is just one of several consumers (see Future usage below).

## Why it is not an application service

It isn't registered in DI, isn't resolved with `GetService<T>()`/constructor injection, and has no
interface. A DI-resolved "current user service" implies the value can vary by registration (swap
the implementation, wrap it, mock it) - `ExecutionContext.Current` is deliberately none of that.
Every consumer, anywhere in the process, sees the exact same value for the current logical request,
because the storage underneath (`AsyncLocal<T>`) flows with the async call chain automatically.
That said, application code that only needs *some* of what's on `ExecutionContext` and prefers an
injectable, mockable abstraction can still keep using `ICurrentUserService` (`BuildingBlock.Web`) -
the two aren't in conflict, they answer different questions for different consumers.

## Why it is initialized only by middleware

`ExecutionContextMiddleware` (`BuildingBlock.Web/Middleware/ExecutionContextMiddleware.cs`) is the
**only** component allowed to read request identity off `HttpContext` - JWT claims, the
`X-Correlation-Id`/`Idempotency-Key`/`Accept-Language` headers. It runs once per request, after
`UseAuthentication`/`UseAuthorization` (so `HttpContext.User`'s claims are populated) and before
every other custom middleware, and calls the framework-only `ExecutionContext.Initialize(...)`.
Every other component - including this same request's own handlers - only ever reads
`ExecutionContext.Current`. Centralizing the HttpContext read in one place means:

- **It parses HttpContext exactly once.** No other component re-parses claims/headers, so there's
  one place that defines what "the current tenant" or "the current correlation id" means.
- **Anonymous requests just work.** Login, Refresh Token, health checks, and public endpoints all
  flow through the same middleware; `UserId`/`TenantId`/`ScopeId` are simply `null` when the
  request carries no authenticated identity. The middleware never rejects a request for missing
  identity - that's an authorization concern, handled elsewhere in the pipeline.
- **CorrelationId is always present.** Generated with `Guid.NewGuid()` when the
  `X-Correlation-Id` header is absent, so every consumer can rely on it being non-empty.
- **IdempotencyKey stays optional.** `null` when the `Idempotency-Key` header is absent - callers
  decide what "missing" means for their own endpoint (see `BuildingBlock.Infrastructure`'s
  `IdempotencyMiddleware`, a separate, unrelated mechanism - see the note under Future usage).

## Execution Context is read-only

`ExecutionContextData` only has `init` setters, and `ExecutionContext.Initialize(...)` is
documented (not enforced by the compiler - there's no `InternalsVisibleTo` gate) as
middleware-only. No handler, application service, or background job may call it or otherwise
mutate the current request's identity mid-request. If a value genuinely needs to change (e.g. a
saga acting on behalf of a different tenant), that is a new, explicit unit of work - never a
mutation of the ambient one.

## Why DbContext contains no request-related dependency injection

`DbContextBase` (`BuildingBlock.Persistence.Ef/DbContext/DbContextBase.cs`) - and `AuthDbContext`,
which can't inherit it - do not inject `ICurrentTenantService`, do not touch `HttpContext`, and
never call `this.GetService<T>()` for anything request-scoped. A DbContext's job is ORM
configuration; it has no business knowing *where* `TenantId` or `UserId` come from. Everything
that used to require that knowledge (the Tenant Convention's query filter,
`TenantAssignmentInterceptor`'s automatic assignment) now reads `ExecutionContext.Current` directly
- a static, zero-dependency read, not a DI resolution. See `docs/reference/tenant-convention.md`
for how the Entity Convention consumes it.

## Future usage

`ExecutionContext.Current` is meant to be the one place every framework component reads identity
from going forward:

- EF interceptors and the Entity Convention (Tenant/Scope/SoftDelete) - already wired, see
  `docs/reference/tenant-convention.md`.
- Audit, Outbox, Inbox - not wired yet; they still use their own metadata providers
  (`IAuditMetadataProvider`, etc.), which remain valid DI-resolved abstractions for now. Wiring
  them onto `ExecutionContext` instead is later work, not part of this refactor.
- Mapster configuration - not wired yet.
- `BuildingBlock.Infrastructure`'s `IdempotencyMiddleware`/`IIdempotencyStore` (the request-level
  Idempotency-Key dedupe cache) is a separate, older mechanism and is unaffected by this work - it
  keeps reading the header itself for now. `ExecutionContext.Current.IdempotencyKey` exists so a
  future pass can consolidate onto it instead of a second independent header read.
