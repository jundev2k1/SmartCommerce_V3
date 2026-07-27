using BuildingBlock.Persistence.Ef.DbContext;
using BuildingBlock.Persistence.Ef.Inbox;
using BuildingBlock.Persistence.Ef.Outbox;

using Order.Persistence.Saga;

namespace Order.Persistence.Engine;

public sealed class OrderDbContext(DbContextOptions<OrderDbContext> options)
    : DbContextBase(options),
    IOutboxDbContext,
    IInboxDbContext
{
    public DbSet<OrderEntity> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<OrderOwner> OrderOwners { get; set; } = null!;
    public DbSet<OrderProductCatalog> OrderProductCatalogs { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;
    public DbSet<SagaExecutionRecordEntity> SagaExecutionRecords { get; set; } = null!;
}
