using System.Linq.Expressions;
using System.Reflection;

using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;
using NovaCore.BuildingBlock.SharedKernel.Context;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.BuildingBlock.Persistence.Ef.DbContext;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies all IEntityTypeConfiguration implementations found in the given assembly. Entity
    /// Conventions (Tenant/Scope/SoftDelete/Idempotent - see ApplyEntityConventions) are applied
    /// separately, since they operate on the whole model rather than one assembly's configs.
    /// </summary>
    public static ModelBuilder ApplyPersistenceConfigurations(
        this ModelBuilder modelBuilder,
        Assembly assembly)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(assembly);

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

    /// <summary>
    /// The Entity Convention: scans every entity type in the model exactly once and applies the
    /// matching EF mapping/indexing/query-filtering for each capability interface it implements
    /// (ITenantEntity, IScopeEntity, ISoftDeleteEntity, IIdempotentEntity) - no entity, and no
    /// per-service IEntityTypeConfiguration, ever configures these by hand. An entity opts in
    /// purely by implementing the interface; it may implement any combination of them.
    ///
    /// TenantId/ScopeId are compared against NovaCore.BuildingBlock.SharedKernel.Context.
    /// ExecutionContext.Current - the ambient, request-scoped identity initialized once by
    /// ExecutionContextMiddleware - never a DI-resolved service and never the DbContext instance
    /// itself. Falls back to Guid.Empty when no tenant/scope is present on the current request
    /// (anonymous requests, background jobs, before token issuance emits these claims), which is
    /// also what TenantAssignmentInterceptor assigns in that same situation - so the filter and
    /// the assignment always agree.
    ///
    /// EF Core only allows one HasQueryFilter per entity type, so filters from every applicable
    /// capability are ANDed together into a single predicate per entity rather than calling
    /// HasQueryFilter once per capability (which would silently overwrite, not combine).
    /// </summary>
    public static ModelBuilder ApplyEntityConventions(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var parameter = Expression.Parameter(clrType, "e");
            Expression? filter = null;

            if (typeof(ITenantEntity).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType).HasIndex(nameof(ITenantEntity.TenantId));
                filter = Combine(filter, EqualTo(parameter, nameof(ITenantEntity.TenantId), TenantComparand.Body));
            }

            if (typeof(IScopeEntity).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType).HasIndex(nameof(IScopeEntity.ScopeId));
                filter = Combine(filter, EqualTo(parameter, nameof(IScopeEntity.ScopeId), ScopeComparand.Body));
            }

            if (typeof(ISoftDeleteEntity).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType).HasIndex(nameof(ISoftDeleteEntity.IsDeleted));
                var isDeleted = Expression.Property(parameter, nameof(ISoftDeleteEntity.IsDeleted));
                filter = Combine(filter, Expression.Not(isDeleted));
            }

            if (typeof(IIdempotentEntity).IsAssignableFrom(clrType))
                modelBuilder.Entity(clrType).HasIndex(nameof(IIdempotentEntity.IdempotencyKey));

            if (filter is not null)
                modelBuilder.Entity(clrType).HasQueryFilter(Expression.Lambda(filter, parameter));
        }

        return modelBuilder;
    }

    private static readonly Expression<Func<Guid>> TenantComparand = () => ExecutionContext.Current.TenantId ?? Guid.Empty;
    private static readonly Expression<Func<Guid>> ScopeComparand = () => ExecutionContext.Current.ScopeId ?? Guid.Empty;

    private static Expression EqualTo(ParameterExpression parameter, string propertyName, Expression comparand) =>
        Expression.Equal(Expression.Property(parameter, propertyName), comparand);

    private static Expression Combine(Expression? left, Expression right) =>
        left is null ? right : Expression.AndAlso(left, right);
}
