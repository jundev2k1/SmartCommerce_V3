namespace NovaCore.BuildingBlock.Persistence.Ef.DbContext;

/// <summary>
/// Implemented by every DbContext the Tenant Convention's dynamically-built query filters target
/// (DbContextBase, and AuthDbContext manually since it can't inherit DbContextBase). CurrentTenantId
/// must be a real instance member - referencing it (not a captured value) inside a HasQueryFilter
/// expression is what lets EF Core late-bind to whichever DbContext instance is actually executing
/// the query, despite the compiled model itself being built once and cached. See
/// docs/reference/tenant-convention.md.
/// </summary>
public interface ITenantAwareDbContext
{
    Guid CurrentTenantId { get; }
}
