using System.Reflection;

using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;

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
        // not here.
    }
}
