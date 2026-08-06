using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.SharedKernel.Context;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace NovaCore.BuildingBlock.Persistence.Ef.Interceptors;

/// <summary>
/// Automatic Entity Convention assignment for every Added entity that implements ITenantEntity
/// and/or IScopeEntity - the only place TenantId/ScopeId are ever set (see
/// ITenantEntity.AssignTenant / IScopeEntity.AssignScope, which this is the sole caller of).
/// Reads NovaCore.BuildingBlock.SharedKernel.Context.ExecutionContext.Current directly - no DI
/// dependency, no ICurrentTenantService - so it stays a plain, stateless interceptor. Falls back
/// to Guid.Empty when the current request carries no tenant/scope (matches the Entity Convention's
/// query filter default in ModelBuilderExtensions, so assignment and filtering never disagree).
/// Both assignment methods are idempotent by construction, so an entity that needs an explicit
/// tenant/scope at construction time (e.g. Scope itself) is never overwritten here.
/// </summary>
public sealed class TenantAssignmentInterceptor : ISaveChangesInterceptor
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

    private static void Assign(Microsoft.EntityFrameworkCore.DbContext? context)
    {
        if (context is null)
            return;

        var current = ExecutionContext.Current;

        foreach (var entry in context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
        {
            if (entry.Entity is ITenantEntity tenantEntity)
                tenantEntity.AssignTenant(current.TenantId ?? Guid.Empty);

            if (entry.Entity is IScopeEntity scopeEntity)
                scopeEntity.AssignScope(current.ScopeId ?? Guid.Empty);
        }
    }
}
