using System.Collections.Concurrent;

using NovaCore.BuildingBlock.Domain.Abstractions;

namespace NovaCore.BuildingBlock.Persistence.Tenancy;

/// <summary>
/// An entity type participates in the Tenant Convention when it extends BaseEntity and does not
/// implement IGlobalEntity - no manual per-entity registration needed (unlike the audit hierarchy,
/// whose parent/child graph genuinely isn't derivable from types alone), so this is a pure
/// computed-and-cached type check rather than a builder-configured registry.
/// </summary>
public sealed class TenantConventionRegistry : ITenantConventionRegistry
{
    private readonly ConcurrentDictionary<Type, bool> _cache = new();

    public bool IsTenantScoped(Type entityType) =>
        _cache.GetOrAdd(entityType, static t =>
            typeof(BaseEntity).IsAssignableFrom(t) && !typeof(IGlobalEntity).IsAssignableFrom(t));
}
