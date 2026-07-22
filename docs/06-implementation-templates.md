# Implementation Templates

**Scope:** copy-paste starting points matching the conventions in [04-coding-rules.md](04-coding-rules.md). Replace `{Service}`, `{Feature}`, `{Entity}`, `{Verb}` placeholders. These mirror Auth Service's actual code — when in doubt, open the cited real file instead of guessing from the template.

## Command + Handler + Validator

`{Service}.Application/Features/{Feature}/Commands/{Verb}/{Verb}Command.cs`
```csharp
namespace {Service}.Application.Features.{Feature}.Commands.{Verb};

public record {Verb}Command(/* inputs */) : ICommand<{Verb}Result>;

public record {Verb}Result(/* outputs */);
```

`{Verb}Handler.cs`
```csharp
namespace {Service}.Application.Features.{Feature}.Commands.{Verb};

public sealed class {Verb}Handler(
    I{Entity}Repository repository,   // or IRepository<{Entity}> if no specific interface needed
    IUnitOfWork unitOfWork) : ICommandHandler<{Verb}Command, {Verb}Result>
{
    public async Task<{Verb}Result> Handle({Verb}Command request, CancellationToken ct = default)
    {
        // 1. Load/validate against current state (throw Application/Domain exceptions on failure)
        // 2. Mutate/create entity
        // 3. Persist (repository call + unitOfWork.SaveChangesAsync(ct) if not auto-saved)
        // 4. Return result
        throw new NotImplementedException();
    }
}
```

`{Verb}Validator.cs` (only if the command has input worth validating)
```csharp
namespace {Service}.Application.Features.{Feature}.Commands.{Verb};

public sealed class {Verb}Validator : AbstractValidator<{Verb}Command>
{
    public {Verb}Validator()
    {
        RuleFor(x => x.SomeField).NotEmpty().WithMessage("SomeField is required");
    }
}
```
Reference: `Auth.Application/Features/Auth/Commands/Register/{RegisterCommand,RegisterHandler,RegisterValidator}.cs`.

## Query + Handler

`{Service}.Application/Features/{Feature}/Queries/{Verb}/{Verb}Query.cs`
```csharp
namespace {Service}.Application.Features.{Feature}.Queries.{Verb};

public record {Verb}Query(/* inputs, e.g. Guid Id */) : IQuery<{Verb}Result>;

public record {Verb}Result(/* outputs */);
```

`{Verb}Handler.cs`
```csharp
namespace {Service}.Application.Features.{Feature}.Queries.{Verb};

public sealed class {Verb}Handler(I{Entity}Repository repository) : IQueryHandler<{Verb}Query, {Verb}Result>
{
    public async Task<{Verb}Result> Handle({Verb}Query request, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("{Entity}", request.Id);

        return new {Verb}Result(/* map fields by hand — Mapster is registered but unused, see coding-rules */);
    }
}
```

## Carter Endpoint

`{Service}.API/Endpoints/{Verb}{Entity}.cs`
```csharp
namespace {Service}.API.Endpoints;

public sealed class {Verb}{Entity} : ICarterModule
{
    private readonly string[] API_DESC = [
        "## {Verb} {Entity}",
        "",
        "Describe what this does.",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/{route}", async (
            [FromBody] {Verb}Request request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new {Verb}Command(request.Field1, request.Field2);
            var result = await sender.Send(command, ct);
            return ApiResponse<{Verb}Result>.Ok(result);
        })
        .WithSummary("{Service}_{Verb}{Entity}")
        .WithDisplayName("{Verb} {Entity} API")
        .WithDescription(API_DESC.JoinToString("\n"))
        .Produces<ApiResponse<{Verb}Result>>();
        // .AllowAnonymous() only if this endpoint must bypass auth
    }
}

public record {Verb}Request(/* HTTP-facing shape, may differ from the command */);
```
Reference: `Auth.API/Endpoints/Register.cs`, `User.API/Endpoints/CreateUser.cs`. Full endpoint-adding checklist: [workflows/add-new-api.md](workflows/add-new-api.md).

## Repository interface + implementation

`{Service}.Application/Abstractions/Repositories/I{Entity}Repository.cs` (only if you need queries beyond generic CRUD — otherwise inject `IRepository<{Entity}>` directly)
```csharp
namespace {Service}.Application.Abstractions.Repositories;

public interface I{Entity}Repository : IRepository<{Entity}>
{
    Task<{Entity}?> GetBySomeFieldAsync(string value, CancellationToken ct = default);
}
```

`{Service}.Persistence/Repositories/{Entity}Repo.cs`
```csharp
namespace {Service}.Persistence.Repositories;

public sealed class {Entity}Repo({Service}DbContext dbContext) : IRepository<{Entity}>, I{Entity}Repository
{
    public async Task<{Entity}?> GetByIdAsync(object id, CancellationToken ct = default)
        => await dbContext.Set<{Entity}>().FirstOrDefaultAsync(x => x.Id.Equals(id), ct);

    public async Task<{Entity}?> GetBySomeFieldAsync(string value, CancellationToken ct = default)
        => await dbContext.Set<{Entity}>().FirstOrDefaultAsync(x => x.SomeField == value, ct);

    // AddAsync / UpdateAsync / DeleteAsync — see BuildingBlock.Application.Abstractions.Persistence.IRepository<T>
}
```
No manual DI registration needed — `AddScopedByInterface(typeof(IRepository<>), typeof({Service}DbContext))` in `{Service}.Persistence/DependencyInjection.cs` picks it up by Scrutor scan. Full checklist: [workflows/add-new-repository.md](workflows/add-new-repository.md).

## Domain entity

This is the minimal shape — a flat entity with no owned collection. If your aggregate owns a required collection (needs ≥1 child, or a many-to-many relationship to another aggregate root), don't start from this template — see [conventions/domain-coding-conventions.md](conventions/domain-coding-conventions.md) for the collection-owning aggregate shape (`Create(..., IEnumerable<{Child}CreateModel> children)`, `ICollection<T> { get; private set; }` navigation, mapping entities for many-to-many) instead.

`{Service}.Domain/Entities/{Entity}.cs`
```csharp
namespace {Service}.Domain.Entities;

public sealed class {Entity} : BaseEntity<Guid>   // or AggregateRoot<Guid> if it's a transaction/consistency boundary
{
    public string SomeField { get; private set; } = string.Empty;

    private {Entity}() { }   // EF Core

    public static {Entity} Create(string someField)
    {
        if (string.IsNullOrWhiteSpace(someField))
            throw ExceptionFactory.RequiredField(nameof(someField));

        return new {Entity} { Id = Guid.NewGuid(), SomeField = someField };
    }

    // Behavior methods, not public setters, for every state change (see conventions/domain-coding-conventions.md#0).
    // AggregateRoot<TId> does not raise events itself (it's a plain marker base type) — if another part of the
    // system needs to react to a change here, publish that reaction from the Application-layer command handler
    // that calls this method, not from inside the entity. See reference/events.md.
}
```
EF config: `{Service}.Persistence/Config/{Entity}Config.cs` implementing `IEntityTypeConfiguration<{Entity}>`. Full checklist: [workflows/add-new-domain-entity.md](workflows/add-new-domain-entity.md).

## Integration event (publish side)

`BuildingBlock.Contract/Events/{Name}IntegrationEvent.cs`
```csharp
namespace BuildingBlock.Contract.Events;

public sealed class {Name}IntegrationEvent : IIntegrationEvent
{
    public Guid CorrelationId { get; }
    public string EventType => nameof({Name}IntegrationEvent);
    public DateTime PublishedAt { get; }
    // + payload fields

    public {Name}IntegrationEvent(/* payload */)
    {
        CorrelationId = Guid.NewGuid();
        PublishedAt = DateTime.UtcNow;
    }
}
```
Publish from the same command handler that made the change, via the Outbox — not a direct publish:
```csharp
await outboxStore.EnqueueAsync(new {Name}IntegrationEvent(/* payload */), ct);
await unitOfWork.SaveChangesAsync(ct);   // aggregate change + OutboxMessage row commit together
```
Never call `IEventPublisher.PublishAsync` directly from feature code — that's the lower-level primitive the Outbox relay itself is built on, and bypasses the Outbox's atomicity guarantee. See [reference/events.md](reference/events.md).

## Integration event (consume side)

`{Service}.Infrastructure/Messaging/Consumers/{Name}Consumer.cs`
```csharp
namespace {Service}.Infrastructure.Messaging.Consumers;

public sealed class {Name}Consumer(ISender sender) : IIntegrationEventConsumer
{
    public IReadOnlyList<string> Topics => ["{publishing-service}.{eventtypelowercased}"];

    public async Task HandleAsync(string message, IReadOnlyDictionary<string, string> headers, CancellationToken ct = default)
    {
        var evt = JsonSerializer.Deserialize<{Name}IntegrationEvent>(message, JsonSerializerConfiguration.Default)!;
        await sender.Send(new {SomeCommand}(evt./* fields */), ct);
        // Adapter only — no business logic here, translate to a command and dispatch.
    }
}
```
Register: `services.AddScoped<IIntegrationEventConsumer, {Name}Consumer>()` in `{Service}.Infrastructure/DependencyInjection.cs`, **before** `AddKafkaMessaging(...)` (topic discovery is eager). Full checklist: [workflows/add-integration-event.md](workflows/add-integration-event.md).

## Background job

`{Service}.Infrastructure/BackgroundJobs/Jobs/{JobName}/{JobName}Service.cs`
```csharp
namespace {Service}.Infrastructure.BackgroundJobs.Jobs.{JobName};

public sealed class {JobName}Service(/* deps */) : IRecurringJob
{
    public string JobId => "{service}-{jobname}";
    public string CronExpression => "*/5 * * * *";
    public string Queue => JobQueue.DEFAULT;
    public bool IsInit => false;

    public async Task ExecuteAsync(CancellationToken ct)
    {
        // do the work
    }
}
```
Register: `services.AddScopedByInterfaceAndConcrete<IRecurringJob>(typeof(DependencyInjection))` — already wired at the `AddBackgroundJobs()` level in services that use Hangfire (currently only Auth). Full checklist: [workflows/add-background-job.md](workflows/add-background-job.md).
