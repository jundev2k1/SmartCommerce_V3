# Tenant Convention

**Scope:** the framework-owned mechanism that gives every entity tenant ownership automatically, without per-entity configuration. Lives in `BuildingBlock.Domain` (contracts), `BuildingBlock.Persistence` (the registry), `BuildingBlock.Application` (the ambient accessor abstraction), and `BuildingBlock.Persistence.Ef` (EF wiring - mapping, indexing, query filtering, automatic assignment).

## Why TenantId lives on BaseEntity

Earlier in this effort, tenant ownership was opt-in: an `ITenantEntity` marker interface, a hand-declared `TenantId` property, a `Guid tenantId` parameter threaded through every `Create()` factory, and a manual `.Property(x => x.TenantId)` + `.HasIndex(...)` pair in every EF config. That put the burden on every new entity to remember to opt in - the wrong default for a platform where the overwhelming majority of business data belongs to exactly one tenant.

`TenantId` was moved onto `BaseEntity` itself (`BuildingBlock.Domain/Abstractions/BaseEntity.cs`) so tenant ownership is a platform capability, not a per-entity decision. `BaseEntity<T>` now inherits `BaseEntity` and adds only `Id`:

```csharp
public abstract class BaseEntity : IEntity
{
    public Guid TenantId { get; private set; }
    // CreatedAt, UpdatedAt, Track(), Touch(), AssignTenant(Guid) ...
}

public abstract class BaseEntity<T> : BaseEntity, IEntity<T>
{
    public T Id { get; init; } = default!;
}
```

`TenantId`'s setter is private - the only sanctioned way to assign it is `BaseEntity.AssignTenant(Guid tenantId)`, an idempotent method (no-op once already assigned) called exclusively by `TenantAssignmentInterceptor`. Business code never assigns `TenantId` directly; a `Create()` factory does not take a `tenantId` parameter for the common case.

## Why ITenantEntity was removed

An opt-in interface meant every new aggregate could silently forget to implement it, and the interface itself carried no behavior worth abstracting - "does this type extend BaseEntity" is a complete answer to "does this type participate in tenant ownership." Removing it eliminates an entire class of omission bugs at the cost of nothing: the rare entity that genuinely isn't tenant-owned opts *out* instead (see `IGlobalEntity` below), which is both rarer and more visible in review than an opt-in that's easy to miss.

## IGlobalEntity

```csharp
public interface IGlobalEntity
{
}
```

Opt-out marker for platform-wide reference data with no tenant meaning. An entity implementing this interface is skipped entirely by the Tenant Convention - no `TenantId` column (via `.Ignore(...)`), no index, no query filter, no automatic assignment.

Marked so far:

- **Auth**: `Tenant`, `TenantLocale` (a Tenant doesn't belong to a tenant - it *is* one), `PermissionDefinition`/`PermissionDefinitionTranslation`, `PermissionGroup`/`PermissionGroupTranslation` (platform-wide permission catalog, seeded by the system).
- **Product**: `SpecificationDefinition`/`Translation`, `SpecificationGroup`/`Translation`, `ProductOptionDefinition`/`Translation`, `ProductOptionValueDefinition`/`Translation` - all explicitly documented in their own class comments as system-managed/globally-shared catalog structure, not seller-owned.
- **Notification**: `NotificationChannel` - platform-seeded delivery integration config (SMTP host, bot token, ...), never tenant-created.

`Tenant` is the one case where `IGlobalEntity` and a real identity collide: `Tenant`'s own `TenantId` *is* its identity (it extends non-generic `AggregateRoot`, not `AggregateRoot<Guid>` - there's no separate `Id`). `TenantConfig` repurposes the inherited `TenantId` as the primary key (`HasKey(x => x.TenantId)`, mapped to the `id` column). The Tenant Convention's ignore step checks whether `TenantId` is already part of an entity's configured primary key before calling `.Ignore(...)`, so this explicit repurposing is respected instead of conflicting with it.

## ISoftDeleteEntity

```csharp
public interface ISoftDeleteEntity
{
}
```

Reserved marker for a future global soft-delete pipeline. Nothing implements it yet, and nothing in the persistence pipeline reads it yet - it exists now so the shape is settled ahead of that work. Deliberately independent of `IAuditable`/audit tracking, which remains its own concern (see `docs/services/auth-service.md` and the Audit Convention below) - soft-delete and audit answer different questions and are not merged into one system.

## Tenant Convention responsibilities

Centralized in `BuildingBlock.Persistence.Tenancy` (the registry) and `BuildingBlock.Persistence.Ef` (everything EF-specific), mirroring the existing `IAuditHierarchyRegistry`/`AuditInterceptor` pattern for the structurally identical audit problem.

### Entity metadata (`ITenantConventionRegistry`)

```csharp
public interface ITenantConventionRegistry
{
    bool IsTenantScoped(Type entityType);
}
```

`TenantConventionRegistry` computes and caches the answer once per type (`typeof(BaseEntity).IsAssignableFrom(t) && !typeof(IGlobalEntity).IsAssignableFrom(t)`), consumed by both the EF model-building convention and the save interceptor - neither repeats the type check independently. Unlike the audit hierarchy, no builder/manual registration exists here: whether a type is tenant-scoped is fully derivable from its CLR type, so there's nothing to configure per service.

### Automatic mapping and indexing

`ModelBuilderExtensions.ApplyTenantConvention` (`BuildingBlock.Persistence.Ef/DbContext/ModelBuilderExtensions.cs`), called from `DbContextBase.OnModelCreating` (and manually from `AuthDbContext.OnModelCreating`, which can't inherit `DbContextBase`) right after `ApplyPersistenceConfigurations`. Loops every entity type in the model:

- Not a `BaseEntity` at all (e.g. Outbox/Inbox/Saga POCOs) → untouched, out of scope entirely.
- `BaseEntity` + not tenant-scoped (`IGlobalEntity`) → `TenantId` ignored, unless already part of the entity's own configured primary key (the `Tenant` case above).
- `BaseEntity` + tenant-scoped → indexed (`HasIndex(nameof(BaseEntity.TenantId))`) and query-filtered (below). EF's own convention-based mapping already creates the column - no explicit `.Property(...)` call is needed for the common case.

No `IEntityTypeConfiguration` ever declares `.Property(x => x.TenantId)` or `.HasIndex(x => x.TenantId)` for the common case - that would just be redundant with what the convention already does.

### Automatic query filter

Built dynamically per entity type via `System.Linq.Expressions`, using EF Core's `EntityTypeBuilder.HasQueryFilter(LambdaExpression)` overload (no compile-time generic needed for a runtime type loop):

```csharp
// e => e.TenantId == context.CurrentTenantId
```

The right-hand side closes over the executing `DbContext` instance (`ITenantAwareDbContext.CurrentTenantId`), not a captured value - EF Core re-evaluates instance-member references in a query filter against whichever `DbContext` instance is actually executing the query, so the filter stays correct per-request even though the compiled `IModel` is built once and cached across every instance of that context type. `DbContextBase.CurrentTenantId` (and `AuthDbContext`'s manual equivalent) resolves `ICurrentTenantService` fresh on every access via `this.GetService<T>()` - never cached on the context itself.

No entity's `IEntityTypeConfiguration` ever calls `HasQueryFilter` for tenant scoping - that would fight the convention, not extend it.

### Automatic tenant assignment

`TenantAssignmentInterceptor` (`BuildingBlock.Persistence.Ef/Interceptors/`), registered alongside `AuditInterceptor`/`TimestampInterceptor`. On `SavingChanges`/`SavingChangesAsync`, for every `Added` entry the registry reports as tenant-scoped, calls `entity.AssignTenant(currentTenant.TenantId)`. Idempotent by construction (`AssignTenant` no-ops once `TenantId` is non-empty), so an entity that *does* need an explicit tenant assignment at construction time (see `Scope` below) is never overwritten.

### The ambient accessor (`ICurrentTenantService`)

```csharp
public interface ICurrentTenantService
{
    Guid TenantId { get; }
}
```

Lives in `BuildingBlock.Application/Abstractions/Services/` - the same layer as `ICurrentUserService` - so `Persistence.Ef` consumes it exactly the way it already consumes `IAuditMetadataProvider`. `NullCurrentTenantService` (`TenantId => Guid.Empty`) is the only implementation registered today, via `TryAddScoped` in `AddPersistenceDbContext`. There is no JWT tenant claim and no Kafka message-header propagation yet - that's explicitly later work (bootstrap/auth phases). Until it exists, every entity's `TenantId` defaults to `Guid.Empty`, and the query filter (`WHERE tenant_id = '00000000-...'`) matches every row consistently, so nothing observable changes today. The mechanism is fully wired; the ambient value it reads is just always the same placeholder for now.

## Rare exceptions customize themselves

The convention's default (implicit assignment from `ICurrentTenantService`) fits the common HTTP-request-scoped case. A small number of aggregates have a genuine business reason to receive an *explicit* tenant at construction time instead - `Scope` (Auth) is the first: which `Tenant` a `Scope` belongs to is a deliberate administrative choice, not necessarily "whoever is currently logged in." `Scope.Create` keeps an explicit `Guid tenantId` parameter and calls `scope.AssignTenant(tenantId)` itself; `TenantAssignmentInterceptor` then no-ops for it (already assigned). This is the "rare exceptions customize themselves" principle in practice - the framework doesn't have a separate code path for this, `AssignTenant`'s idempotency is what makes both paths coexist safely.

## Why audit remains independent

`IAuditable`/`AuditInterceptor`/`IAuditHierarchyRegistry` are untouched by this work. Audit answers "what changed, when, and who changed it" across a manually-registered parent/child hierarchy (real graph traversal, not derivable from types alone). The Tenant Convention answers "which tenant owns this row" via a pure type check. The two happen to share the same `ISaveChangesInterceptor`/registry-plus-cache shape because that shape is the right one for "framework-owned, type-driven cross-cutting concern in this codebase" - not because the concerns themselves are related. Merging them would couple two independently-evolving systems (a future soft-delete pipeline, or the eventual real `ICurrentTenantService`, must never need to touch audit code, and vice versa).
