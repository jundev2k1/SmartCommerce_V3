namespace NovaCore.BuildingBlock.Persistence.Tenancy;

/// <summary>
/// Read-only lookup of whether an entity type participates in the Tenant Convention (TenantId
/// mapping/indexing/query-filtering/automatic assignment) - computed once per type and cached,
/// consumed throughout the persistence pipeline instead of each component repeating the same
/// BaseEntity/IGlobalEntity type checks.
/// </summary>
public interface ITenantConventionRegistry
{
    bool IsTenantScoped(Type entityType);
}
