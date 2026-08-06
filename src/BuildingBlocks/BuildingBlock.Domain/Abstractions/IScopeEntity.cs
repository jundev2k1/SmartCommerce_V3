namespace NovaCore.BuildingBlock.Domain.Abstractions;

/// <summary>
/// Opt-in capability for entities owned by a Scope (Branch, Agency, Dealer, Region, ...) within a
/// Tenant. Mirrors ITenantEntity exactly - implementing this interface is what makes the Entity
/// Convention map/index/query-filter ScopeId automatically. Independent of ITenantEntity: an
/// entity may implement one, the other, both, or neither, depending on what actually owns it.
/// </summary>
public interface IScopeEntity
{
    Guid ScopeId { get; }

    /// <summary>Framework-only assignment point - must be idempotent (no-op once already assigned),
    /// same reasoning as ITenantEntity.AssignTenant.</summary>
    void AssignScope(Guid scopeId);
}
