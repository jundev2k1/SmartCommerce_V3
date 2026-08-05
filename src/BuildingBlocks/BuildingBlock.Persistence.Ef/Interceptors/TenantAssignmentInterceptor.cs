using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Persistence.Tenancy;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace NovaCore.BuildingBlock.Persistence.Ef.Interceptors;

/// <summary>
/// Automatic Tenant Convention assignment - the only place TenantId is ever set. Business code
/// never assigns it directly (see BaseEntity.AssignTenant, which this is the sole caller of).
/// Mirrors TimestampInterceptor's shape exactly.
/// </summary>
public sealed class TenantAssignmentInterceptor(
    ITenantConventionRegistry registry,
    ICurrentTenantService currentTenant) : ISaveChangesInterceptor
{
    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Assign(eventData.Context);

        return result;
    }

    public ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        Assign(eventData.Context);

        return ValueTask.FromResult(result);
    }

    private void Assign(Microsoft.EntityFrameworkCore.DbContext? context)
    {
        if (context is null)
            return;

        var entries = context.ChangeTracker.Entries<BaseEntity>()
            .Where(entry => entry.State == EntityState.Added && registry.IsTenantScoped(entry.Entity.GetType()));

        foreach (var entry in entries)
            entry.Entity.AssignTenant(currentTenant.TenantId);
    }
}
