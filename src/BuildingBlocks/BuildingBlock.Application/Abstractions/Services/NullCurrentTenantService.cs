namespace NovaCore.BuildingBlock.Application.Abstractions.Services;

/// <summary>Default no-op accessor - registered so the Tenant Convention never fails to resolve DI before real tenant resolution exists.</summary>
public sealed class NullCurrentTenantService : ICurrentTenantService
{
    public Guid TenantId => Guid.Empty;
}
