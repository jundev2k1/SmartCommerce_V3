# Entity Convention (Tenant / Scope / SoftDelete / Idempotent)

**Scope:** the framework-owned mechanism that applies EF mapping/indexing/query-filtering
automatically to any entity that opts into one of four capabilities, with zero per-entity EF
configuration. Lives in `BuildingBlock.Domain` (the capability interfaces) and
`BuildingBlock.Persistence.Ef` (the scan itself - `ModelBuilderExtensions.ApplyEntityConventions`,
called from `DbContextBase.OnModelCreating` and, manually, from `AuthDbContext.OnModelCreating`,
which can't inherit `DbContextBase`).

## Why this is opt-in, not a BaseEntity default

An earlier version of this convention put `TenantId` directly on `BaseEntity`, so every entity got
it unconditionally, with an opt-*out* marker (`IGlobalEntity`) for the rare platform-wide entity.
That inverted the actual shape of the platform: it made "does this entity have a Scope, or need
soft-delete, or carry an idempotency key" impossible to express at all (there was only ever a
Tenant slot), and treated the common case (tenant-owned) and the exception (global) asymmetrically
from every other capability. `BaseEntity` (`BuildingBlock.Domain/Abstractions/BaseEntity.cs`) now
carries only what every entity genuinely needs - `CreatedAt`/`UpdatedAt`/`Track()`/`Touch()`.
Everything else is a capability interface an entity implements only if it actually needs it:

```csharp
public interface ITenantEntity
{
    Guid TenantId { get; }
    void AssignTenant(Guid tenantId);
}

public interface IScopeEntity
{
    Guid ScopeId { get; }
    void AssignScope(Guid scopeId);
}

public interface ISoftDeleteEntity
{
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }
    void MarkDeleted();
}

public interface IIdempotentEntity
{
    string? IdempotencyKey { get; }
}
```

An entity may implement any combination of these, or none. There is no opt-out marker anymore
(`IGlobalEntity` is gone) - not implementing `ITenantEntity` *is* the opt-out.

`TenantId`'s setter is private per-entity, and the only sanctioned way to assign it is
`AssignTenant(Guid tenantId)` - implemented per entity as a two-line idempotent block (no-op once
already assigned), called exclusively by `TenantAssignmentInterceptor`. Business code never
assigns `TenantId`/`ScopeId` directly. `IScopeEntity` mirrors this exactly for `ScopeId`.

## Rare exceptions customize themselves

The convention's default (implicit assignment from `ExecutionContext.Current`) fits the common
HTTP-request-scoped case. A small number of aggregates have a genuine business reason to receive
an *explicit* tenant at construction time instead - `Scope` (Auth) is the first: which `Tenant` a
`Scope` belongs to is a deliberate administrative choice, not necessarily "whoever is currently
logged in." `Scope.Create` keeps an explicit `Guid tenantId` parameter and calls
`scope.AssignTenant(tenantId)` itself; `TenantAssignmentInterceptor` then no-ops for it (already
assigned). `AssignTenant`'s idempotency is what makes both paths coexist safely.

## The Entity Convention scan

`ModelBuilderExtensions.ApplyEntityConventions` (`BuildingBlock.Persistence.Ef/DbContext/ModelBuilderExtensions.cs`)
loops every entity type in the model exactly once and, per type, checks each capability interface
via a plain `IsAssignableFrom` - no registry, no caching, since this only runs at model-build time
(once per process), not per request:

- `ITenantEntity` → indexes `TenantId`, contributes `e.TenantId == (ExecutionContext.Current.TenantId ?? Guid.Empty)` to the entity's query filter.
- `IScopeEntity` → indexes `ScopeId`, contributes `e.ScopeId == (ExecutionContext.Current.ScopeId ?? Guid.Empty)`.
- `ISoftDeleteEntity` → indexes `IsDeleted`, contributes `!e.IsDeleted`.
- `IIdempotentEntity` → indexes `IdempotencyKey`. No query filter (existence isn't a visibility
  rule); uniqueness scope varies per entity (globally unique, or unique within some other column)
  and stays a per-entity `IEntityTypeConfiguration` concern - the convention only guarantees the
  column is mapped and indexed for lookup, it never assumes a uniqueness shape.

EF Core only allows **one** `HasQueryFilter` call per entity type - calling it more than once
overwrites rather than combines. An entity implementing more than one filtering capability (e.g.
`ITenantEntity` + `ISoftDeleteEntity`) gets all of its applicable predicates ANDed into a single
expression before `HasQueryFilter` is called once. No entity's own `IEntityTypeConfiguration` ever
calls `.Property()`/`.HasIndex()`/`.HasQueryFilter()` for any of these four properties - that would
duplicate, not extend, the convention.

## Reading identity: `ExecutionContext`, not a DI service

`TenantId`/`ScopeId` are compared against
`NovaCore.BuildingBlock.SharedKernel.Context.ExecutionContext.Current` - see
`docs/reference/execution-context.md` for the full picture. This is a static, ambient read, not a
DI-resolved service: the old `ICurrentTenantService`/`NullCurrentTenantService`/
`ITenantConventionRegistry` abstractions are gone, and `DbContextBase` no longer calls
`this.GetService<T>()` for anything request-related. Both the query filter and
`TenantAssignmentInterceptor`'s assignment fall back to `Guid.Empty` when the current request
carries no tenant/scope (no JWT tenant/scope claim yet - token issuance emitting `tenant_id`/
`scope_id` claims is separate, later work), so filtering and assignment always agree: until real
claims flow, every entity defaults to `Guid.Empty` and the filter matches every row consistently,
exactly like the placeholder behavior before this refactor.

## Automatic tenant/scope assignment

`TenantAssignmentInterceptor` (`BuildingBlock.Persistence.Ef/Interceptors/`), registered alongside
`AuditInterceptor`/`TimestampInterceptor`. On `SavingChanges`/`SavingChangesAsync`, for every
`Added` entry, assigns `TenantId` if the entity implements `ITenantEntity` and `ScopeId` if it
implements `IScopeEntity`. It is a plain, stateless class - no constructor dependencies at all -
since it reads `ExecutionContext.Current` directly instead of a DI-resolved service.

## SoftDelete and Idempotent conventions

`ISoftDeleteEntity` and `IIdempotentEntity` get their EF mapping/indexing (and, for SoftDelete, the
query filter) from the same scan. Neither has an assignment interceptor: soft-deleting is a
deliberate business action (`entity.MarkDeleted()`, called from application code, same as any other
state transition), and an idempotency key is normally supplied by the client/caller at creation
time, not derived from ambient request identity the way `TenantId`/`ScopeId` are.

## Why audit remains independent

`IAuditable`/`AuditInterceptor`/`IAuditHierarchyRegistry` are untouched by this convention. Audit
answers "what changed, when, and who changed it" across a manually-registered parent/child
hierarchy (real graph traversal, not derivable from types alone). The Entity Convention answers
"which Tenant/Scope owns this row, is it soft-deleted, does it have a dedupe key" via pure type
checks. The two happen to share a similar interceptor/model-building shape because that shape fits
"framework-owned, type-driven cross-cutting concern in this codebase" generally - not because the
concerns themselves are related.
