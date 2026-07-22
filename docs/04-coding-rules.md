# Coding Rules

**Scope:** conventions extracted from the actual codebase (Auth Service = reference). These are observed rules, not invented ones — every example below cites the file it came from. For layering/dependency rules, see [02-architecture-rules.md](02-architecture-rules.md). For ready-to-copy code shapes, see [06-implementation-templates.md](06-implementation-templates.md). For layer-specific style rules — how a class is shaped, not just where the file goes — see [conventions/domain-coding-conventions.md](conventions/domain-coding-conventions.md) (Domain: aggregate creation shape, no Spec objects, many-to-many mapping entities, reusable Value Object validation) and [conventions/application-coding-conventions.md](conventions/application-coding-conventions.md) (Application: full Feature-First folder shape, Handler Philosophy, responsibility-based extraction, Mapster policy). This doc covers naming, CQRS shape, endpoints, DI registration, and everything else not owned by those two.

## Folder structure (per feature)

Full shape, including `Common/`, `DTOs/`, `Mapping/`, `Utilities/` and the responsibility of each folder: [conventions/application-coding-conventions.md#feature-first-structure](conventions/application-coding-conventions.md#feature-first-structure). Minimal shape for reference:

```
{Service}.Application/
  Abstractions/{Auth,Repositories,Security,Services}/     interfaces only
  Features/{FeatureArea}/
    Commands/{CommandName}/
      {CommandName}Command.cs
      {CommandName}Handler.cs
      {CommandName}Validator.cs        (if the command needs validation)
    Queries/{QueryName}/
      {QueryName}Query.cs
      {QueryName}Handler.cs
    Events/{EventName}/
      {EventName}Event.cs
      {EventName}Handler.cs
  DependencyInjection.cs
  GlobalUsings.cs
```

Example: `Auth.Application/Features/Auth/Commands/Register/{RegisterCommand,RegisterHandler,RegisterValidator}.cs`. One class per file, filename == class name (Carter endpoint files are the one exception — see below).

```
{Service}.Infrastructure/
  {Concern}/  e.g. Caching/, Security/{Jwt,RefreshTokens}/, BackgroundJobs/{Jobs/{JobName},Services}/, Messaging/Consumers/, GrpcClients/
  DependencyInjection.cs

{Service}.Persistence/
  Config/            IEntityTypeConfiguration<T> per entity
  Migrations/
  Repositories/       {Entity}Repo.cs  (implementation, abbreviated name)
  Seeders/
  Services/            concrete app-facing services backed by EF/Identity
  UnitOfWork/
  {Service}DbContext.cs
  DependencyInjection.cs

{Service}.API/
  Endpoints/{FeatureVerb}.cs   one Carter module per endpoint (not per feature folder)
  DependencyInjection.cs        AddPresentation, calls AddBuildingBlockWeb
  ApplicationPipeline.cs        UseApplication, calls UseBuildingBlockWeb
  Program.cs
  GlobalUsings.cs
```

## Naming conventions

- Commands/Queries: `{Verb}Command` / `{Verb}Query`, handler `{Verb}Handler`, validator `{Verb}Validator`, result `{Verb}Result` (record).
- Events: `On{Trigger}Event` / `On{Trigger}Handler` (e.g. `OnUserRegisteredEvent`, `OnUserRegisteredHandler`).
- Repository **interfaces**: `I{Entity}Repository` (full word). Repository **implementations**: `{Entity}Repo` (abbreviated). This asymmetry is intentional house style — keep it.
- Namespaces mirror folder paths exactly (`Auth.Application.Features.Auth.Commands.Register`).
- `GlobalUsings.cs` per project centralizes cross-cutting `global using`s (e.g. CQRS abstractions, Carter, MediatR).
- `ct` is the standard `CancellationToken` parameter name, always trailing, **always defaulted** (`CancellationToken ct = default`) — including on handler/interface method signatures, not just leaf calls. This is atypical MediatR style but is the consistent house convention; follow it everywhere, including new interfaces.

## CQRS shape

```csharp
public record RegisterCommand(...) : ICommand<RegisterResult>;
public record RegisterResult(...);

public sealed class RegisterHandler(IAccountRepository accounts, IUnitOfWork uow, ...) : ICommandHandler<RegisterCommand, RegisterResult>
{
    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken ct = default) { ... }
}
```

Primary-constructor DI. `ICommand`/`ICommandHandler`/`IQuery`/`IQueryHandler` come from `BuildingBlock.Application.Abstractions.CQRS` — always use these, never `MediatR.IRequest` directly, so the pipeline behaviors apply uniformly.

## Validation

- FluentValidation, one validator class per command/query, co-located in the same feature folder.
- Auto-registered via `services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly)` — **do not manually register a validator**; if it's not picked up, the problem is namespace/assembly, not registration.
- Validation failures surface as `BuildingBlock.Application.Exceptions.ValidationException` via `ValidationBehavior<,>` — never catch/rethrow this yourself.

## Mapping

Mapster is registered (`TypeAdapterConfig.GlobalSettings` + `services.AddScoped<IMapper, ServiceMapper>()`) in every service's `Application/DependencyInjection.cs`. **New features use it** for straightforward mapping; existing hand-mapped handlers (Auth/User, and Product/Inventory/Order's first pass) are grandfathered, not retrofitted. Full policy and the reasoning behind it: [conventions/application-coding-conventions.md#mapping](conventions/application-coding-conventions.md#mapping).

## Repository & Unit of Work

- Generic `IRepository<T>` (`BuildingBlock.Application.Abstractions.Persistence.IRepository<T>`) is always available. Auth additionally defines per-aggregate interfaces (`IAccountRepository`) in `Application/Abstractions/Repositories/` when the aggregate needs queries beyond the generic CRUD surface — **follow Auth's pattern**: define a specific interface only when you need methods the generic one doesn't have; otherwise inject `IRepository<T>` directly (User Service's accepted alternative — see [services/user-service.md](services/user-service.md)).
- **An established third option: a standalone repository interface that doesn't implement `IRepository<T>` at all**, for an aggregate whose real query shape (search/filter/rollup/exists-checks, no conditional-include overload) has drifted far enough from generic CRUD that forcing it to still satisfy `IRepository<T>` would add nothing. Product (`IProductRepository`/`IProductCategoryRepository`/`IProductTagRepository`), Inventory (`IInventoryTransactionRepository`), and Audit (`IAuditLogRepository`) all do this — each is registered manually in that service's `Persistence/DependencyInjection.cs` instead of being picked up by the Scrutor scan, since the scan matches `IRepository<T>` implementations specifically. Reach for this only once a repository's real usage looks like this, not preemptively.
- Implementations live in `{Service}.Persistence/Repositories/{Entity}Repo.cs` and implement both the generic and (if present) the specific interface (or, for the standalone case above, just the standalone interface). Registration is automatic via Scrutor scan (`AddScopedByInterface(typeof(IRepository<>), typeof({Service}DbContext))`) for anything implementing `IRepository<T>` — **do not manually register a repository that does**.
- Repositories never call `SaveChanges`. Simple single-write flows rely on the underlying persistence mechanism (e.g. ASP.NET Identity's `UserManager` auto-saves). Everything else follows the Transaction Management rule below.

## Transaction Management

`IUnitOfWork` (`BuildingBlock.Application.Abstractions.Persistence.IUnitOfWork`) exposes exactly two members: `SaveChangesAsync(ct)` and `ExecuteTransactionAsync(Func<Task> action, Func<Task>? rollbackAction = null, CancellationToken ct = default)`. There is no `BeginTransactionAsync`/`CommitAsync`/`RollbackAsync`/`WithBeginTransactionAsync` on the interface — if you've seen those names, they're from an outdated version of this doc; `ExecuteTransactionAsync` is the only transaction primitive.

**Do not rely on EF Core's implicit per-`SaveChanges()` transaction as your transaction boundary.** For any workflow with more than one logical write, or that must stay atomic with a non-EF side effect staged on the same `DbContext` (e.g. an Outbox row), wrap the whole thing in `ExecuteTransactionAsync`:

```csharp
public async Task<Result> Handle(SomeCommand request, CancellationToken ct = default)
{
    await unitOfWork.ExecuteTransactionAsync(async () =>
    {
        await repository.UpdateAsync(request.Id, entity => entity.DoSomething(...), ct);

        await outboxStore.EnqueueAsync(new SomeIntegrationEvent(...), ct);

        await unitOfWork.SaveChangesAsync(ct);   // flushes pending changes — does not own the transaction
    }, ct: ct);

    return new Result(...);
}
```

`ExecuteTransactionAsync` opens the real database transaction (via `Database.BeginTransactionAsync`, wrapped in EF's execution strategy for retry-on-transient-failure), runs `action`, calls `SaveChangesAsync` once more itself after `action` returns, commits, and — on any exception — runs `rollbackAction` (if provided), rolls back, clears the change tracker (so entities touched inside `action` don't stay stuck `Added`/`Modified` after a rollback), and rethrows. **The transaction's lifetime is owned by `ExecuteTransactionAsync`, not by any `SaveChangesAsync` call inside it.**

**Multiple `SaveChangesAsync()` calls inside one `action` are fine** — they're just flushes against the same open transaction, not separate commits — as long as they all happen inside the same `ExecuteTransactionAsync` scope and the workflow commits exactly once, at the end, on success. What's not fine is calling `SaveChangesAsync()` **outside** `ExecuteTransactionAsync`, either instead of it or as a follow-up call after it returns — each bare `SaveChangesAsync()` gets its own implicit, separate transaction, so a crash between two such calls can commit one write and lose the other.

Reference for the correct shape: `StockInHandler`/`StockOutHandler`/`AdjustStockHandler` (`Inventory.Application/Features/Inventories/Commands/`) — the entire aggregate mutation + transaction-log write happens inside one `ExecuteTransactionAsync` call. **Known gap, not yet fixed:** `Product.Application`'s `CreateProductHandler` calls `uow.SaveChangesAsync(ct)` directly, twice, with no `ExecuteTransactionAsync` wrapper at all; `UpdateProductHandler`/the variation update/delete handlers wrap the aggregate mutation in `ExecuteTransactionAsync` but then call `outboxStore.EnqueueAsync` + a second, unwrapped `unitOfWork.SaveChangesAsync(ct)` *after* it returns — the Outbox enqueue is not atomic with the aggregate change in either case. This is documented in [services/product-service.md#known-issues](services/product-service.md#known-issues); fixing it is a code change, not something this convention doc does on its own — don't copy either shape into new handlers.

## Exceptions

See the full rule in [02-architecture-rules.md](02-architecture-rules.md#exception-rule) and the catalogue in [reference/exceptions.md](reference/exceptions.md). Short version: Domain exceptions for business rules, Application exceptions for HTTP-aware failures, never raw BCL exceptions from a handler.

## Endpoints (Carter)

```csharp
public sealed class Register : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/register", async ([FromBody] RegisterRequest request, [FromServices] ISender sender, CancellationToken ct = default) =>
        {
            var command = new RegisterCommand(...);   // hand-mapped from request
            var result = await sender.Send(command, ct);
            return ApiResponse<RegisterResult>.Ok(result);
        })
        .AllowAnonymous()                              // or rely on default auth policy
        .WithSummary("Auth_RegisterUser")
        .WithDescription(API_DESC.JoinToString("\n"))   // Markdown description array, see template
        .Produces<ApiResponse<RegisterResult>>();
    }
}
```

The request DTO (`RegisterRequest`) is a `record` declared in the same file, above the module class. One `ICarterModule` per endpoint file, named after the action (`Register.cs`, `Login.cs`, `GetUser.cs`), not per feature. Endpoints never contain business logic — bind, build command/query, `sender.Send`, return.

## DI registration

One public `Add{Layer}` extension per project (`AddApplication`, `AddInfrastructure`, `AddPersistence`, `AddPresentation`), composed of `private static` helper methods, all chained off `services`. Composition order is fixed — see [02-architecture-rules.md](02-architecture-rules.md#composition-root-convention-per-service). Any reflection-based bulk registration (repositories, background jobs) goes through `BuildingBlock.Infrastructure.Extensions.ServiceScanningExtensions` (Scrutor) — never hand-roll a loop over `Assembly.GetTypes()` elsewhere.

## Caching / decorator pattern

To add caching to an existing service (not a repository), wrap it: define the decorator implementing the same interface, resolve the concrete implementation manually in DI, and register the decorator as the interface. Canonical example: `Auth.Infrastructure/Caching/CachedAuthServiceDecorator.cs` + `AddRoleCaching()` in `Auth.Infrastructure/DependencyInjection.cs`. Full pattern: [reference/caching.md](reference/caching.md).

## Background jobs

Implement `IRecurringJob` (`BuildingBlock.Application.Abstractions.Jobs`), register via `AddScopedByInterfaceAndConcrete<IRecurringJob>`. See [workflows/add-background-job.md](workflows/add-background-job.md).

## Async

All I/O-bound methods are `async Task`/`async Task<T>`, `ct` threaded through every call down to the EF Core / Redis / HTTP call. No `.Result`/`.Wait()` in request-handling code paths (the two known exceptions — `SeedDatabase`/`InitializeRefreshTokenCache` in `Auth.API/ApplicationPipeline.cs` calling `.Wait()` — are startup-only, not request-time, and are an accepted exception to this rule, not a pattern to copy elsewhere).
