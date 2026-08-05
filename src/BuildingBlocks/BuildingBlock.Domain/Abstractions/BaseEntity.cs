using NovaCore.BuildingBlock.Domain.Attributes;

namespace NovaCore.BuildingBlock.Domain.Abstractions;

/// <summary>
/// Base type for every entity in the platform. Tenant ownership is a platform capability, not
/// something each entity opts into - every BaseEntity carries TenantId, and the persistence
/// layer's Tenant Convention (see NovaCore.BuildingBlock.Persistence.Ef) is solely responsible
/// for mapping, indexing, query-filtering, and assigning it automatically. Business code never
/// sets TenantId directly - see AssignTenant. Entities that are platform-wide reference data with
/// no tenant meaning (e.g. system-seeded lookup tables) opt out via IGlobalEntity instead.
/// </summary>
public abstract class BaseEntity : IEntity
{
    public Guid TenantId { get; private set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [AuditIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Framework-only assignment point (TenantAssignmentInterceptor) - idempotent, never
    /// overwrites an already-assigned tenant.</summary>
    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    public void Track()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

public abstract class BaseEntity<T> : BaseEntity, IEntity<T>
{
    public T Id { get; init; } = default!;
}
