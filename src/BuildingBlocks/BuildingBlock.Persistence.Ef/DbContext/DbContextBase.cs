using Microsoft.EntityFrameworkCore;

namespace BuildingBlock.Persistence.Ef.DbContext;

/// <summary>
/// Shared base for every non-Identity EF DbContext in the solution. Centralizes the model/options
/// setup that would otherwise be duplicated in every service's DbContext: applying that context's
/// own IEntityTypeConfiguration classes, applying Outbox/Inbox configuration when applicable, and
/// suppressing the "pending model changes" design-time warning.
///
/// AuthDbContext cannot inherit this - it must inherit IdentityDbContext instead - so it calls
/// the same ModelBuilderExtensions/DbContextOptionsBuilderExtensions helpers this class uses,
/// explicitly, from its own OnConfiguring/OnModelCreating.
/// </summary>
public abstract class DbContextBase(DbContextOptions options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    protected sealed override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.SuppressPendingModelChangesWarning();
    }

    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyPersistenceConfigurations(GetType().Assembly);
        modelBuilder.ApplyOutboxInboxConfiguration(this);

        ConfigureModel(modelBuilder);
    }

    /// <summary>Hook for model configuration a derived context needs beyond assembly scanning + Outbox/Inbox.</summary>
    protected virtual void ConfigureModel(ModelBuilder modelBuilder)
    {
    }

    public bool IsDisableTimestamps { get; set; }
}
