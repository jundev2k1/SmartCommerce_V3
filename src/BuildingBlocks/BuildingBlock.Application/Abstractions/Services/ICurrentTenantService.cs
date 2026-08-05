namespace NovaCore.BuildingBlock.Application.Abstractions.Services;

/// <summary>
/// Ambient current-tenant accessor consumed by the Tenant Convention (query filter + automatic
/// assignment - see NovaCore.BuildingBlock.Persistence.Ef). Lives in Application - not
/// Persistence.Ef - so the real implementation (HTTP/JWT-claim-based, or message-header-based for
/// async consumers) stays decoupled from EF. The default <see cref="NullCurrentTenantService"/> is
/// registered by NovaCore.BuildingBlock.Persistence.Ef so persistence works even before real tenant
/// resolution is wired in (no JWT tenant claim exists yet - that's later, separate work).
/// </summary>
public interface ICurrentTenantService
{
    Guid TenantId { get; }
}
