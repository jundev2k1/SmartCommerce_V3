namespace NovaCore.BuildingBlock.Domain.Abstractions;

/// <summary>
/// Opt-out marker for platform-wide reference data with no tenant meaning (e.g. a system-seeded
/// lookup/definition table). The Tenant Convention (see NovaCore.BuildingBlock.Persistence.Ef)
/// skips TenantId mapping, indexing, query-filter registration, and automatic assignment for any
/// entity implementing this interface. Almost every business entity belongs to a Tenant - this is
/// the rare exception, not the default.
/// </summary>
public interface IGlobalEntity
{
}
