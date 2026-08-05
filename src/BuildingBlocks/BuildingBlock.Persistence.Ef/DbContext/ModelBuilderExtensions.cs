using System.Linq.Expressions;
using System.Reflection;

using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;
using NovaCore.BuildingBlock.Persistence.Tenancy;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.BuildingBlock.Persistence.Ef.DbContext;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies all IEntityTypeConfiguration implementations
    /// and shared EF conventions.
    /// </summary>
    public static ModelBuilder ApplyPersistenceConfigurations(
        this ModelBuilder modelBuilder,
        Assembly assembly)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(assembly);

        ConfigureGlobalConventions(modelBuilder);

        return modelBuilder;
    }

    /// <summary>
    /// Applies OutboxConfiguration/InboxConfiguration when <paramref name="context"/> implements
    /// the corresponding marker interface (IOutboxDbContext/IInboxDbContext). Those configs live
    /// in this project's own Outbox/Inbox namespaces, so ApplyPersistenceConfigurations - scoped
    /// to the derived DbContext's own assembly - never finds them on its own. Every context that
    /// needs Outbox and/or Inbox support must apply them explicitly one way or another, whether
    /// or not it can inherit DbContextBase (e.g. AuthDbContext, which must inherit
    /// IdentityDbContext instead and calls this the same way DbContextBase does internally).
    /// </summary>
    public static ModelBuilder ApplyOutboxInboxConfiguration(
        this ModelBuilder modelBuilder,
        Microsoft.EntityFrameworkCore.DbContext context)
    {
        if (context is IOutboxDbContext)
            modelBuilder.ApplyConfiguration(new OutboxConfiguration());

        if (context is IInboxDbContext)
            modelBuilder.ApplyConfiguration(new InboxConfiguration());

        return modelBuilder;
    }

    private static void ConfigureGlobalConventions(ModelBuilder modelBuilder)
    {
        // Future shared conventions:
        //
        // - Default schema
        // - DeleteBehavior
        // - Decimal precision
        // - Strongly Typed Id
        //
        // Keep empty until actually needed. Naming convention (snake_case) is already handled at
        // the DbContextOptionsBuilder level (see DbContextOptionsBuilderExtensions.UsePersistenceDefaults),
        // not here. Tenant Convention (TenantId mapping/index/query-filter) is ApplyTenantConvention
        // below, kept separate since it needs the executing DbContext instance, not just the ModelBuilder.
    }

    /// <summary>
    /// Automatic Tenant Convention: every entity type extending BaseEntity gets TenantId indexed
    /// and query-filtered against the executing DbContext's CurrentTenantId; IGlobalEntity types
    /// have TenantId ignored entirely (no column, no filter). No per-entity configuration is ever
    /// required - see docs/reference/tenant-convention.md.
    /// </summary>
    public static ModelBuilder ApplyTenantConvention(
        this ModelBuilder modelBuilder,
        ITenantAwareDbContext context,
        ITenantConventionRegistry registry)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (!typeof(BaseEntity).IsAssignableFrom(clrType))
                continue;

            if (!registry.IsTenantScoped(clrType))
            {
                // A rare IGlobalEntity (e.g. Tenant itself) may deliberately repurpose the
                // inherited TenantId as its own explicit key/identity via its own
                // IEntityTypeConfiguration - respect that instead of blindly ignoring a property
                // already wired into the primary key.
                var alreadyKeyed = entityType.FindPrimaryKey()?.Properties
                    .Any(p => p.Name == nameof(BaseEntity.TenantId)) == true;

                if (!alreadyKeyed)
                    modelBuilder.Entity(clrType).Ignore(nameof(BaseEntity.TenantId));

                continue;
            }

            modelBuilder.Entity(clrType).HasIndex(nameof(BaseEntity.TenantId));
            modelBuilder.Entity(clrType).HasQueryFilter(BuildTenantFilter(clrType, context));
        }

        return modelBuilder;
    }

    /// <summary>
    /// Builds `e => e.TenantId == context.CurrentTenantId` for the given CLR type. The right-hand
    /// side deliberately closes over `context` (an instance member access), not a captured value -
    /// EF Core re-evaluates instance-member references in a query filter against whichever
    /// DbContext instance is actually executing the query, so this stays correct per-request
    /// despite the compiled model being built once and cached across instances.
    /// </summary>
    private static LambdaExpression BuildTenantFilter(Type clrType, ITenantAwareDbContext context)
    {
        var parameter = Expression.Parameter(clrType, "e");
        var tenantIdProperty = Expression.Property(parameter, nameof(BaseEntity.TenantId));

        var contextConstant = Expression.Constant(context, typeof(ITenantAwareDbContext));
        var currentTenantIdProperty = Expression.Property(contextConstant, nameof(ITenantAwareDbContext.CurrentTenantId));

        var body = Expression.Equal(tenantIdProperty, currentTenantIdProperty);

        return Expression.Lambda(body, parameter);
    }
}
