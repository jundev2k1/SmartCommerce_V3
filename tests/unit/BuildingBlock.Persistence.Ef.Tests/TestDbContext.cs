using SmartEcommerce.BuildingBlock.Persistence.Ef.Outbox;

using Microsoft.EntityFrameworkCore;

namespace SmartEcommerce.BuildingBlock.Persistence.Ef.Tests;

// Fully-qualified DbContext: this project's namespace nests under SmartEcommerce.BuildingBlock.Persistence.Ef,
// which also contains a sibling SmartEcommerce.BuildingBlock.Persistence.Ef.DbContext *namespace* - the bare
// name would otherwise resolve there instead of Microsoft.EntityFrameworkCore.DbContext, same
// reason DbContextBase.cs in the main project fully-qualifies it too.
public sealed class TestDbContext(DbContextOptions<TestDbContext> options)
    : Microsoft.EntityFrameworkCore.DbContext(options), IOutboxDbContext
{
    public DbSet<TestOrder> Orders => Set<TestOrder>();
    public DbSet<TestOrderItem> OrderItems => Set<TestOrderItem>();
    public DbSet<TestOrderTax> OrderTaxes => Set<TestOrderTax>();
    public DbSet<TestUser> Users => Set<TestUser>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new OutboxConfiguration());
    }
}
