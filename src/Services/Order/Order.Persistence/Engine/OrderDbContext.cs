using SmartEcommerce.BuildingBlock.Persistence.Ef.DbContext;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Inbox;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Outbox;

using SmartEcommerce.Order.Persistence.Reliability.Saga;

namespace SmartEcommerce.Order.Persistence.Engine;

public sealed class OrderDbContext(DbContextOptions<OrderDbContext> options)
    : DbContextBase(options),
    IOutboxDbContext,
    IInboxDbContext
{
    public DbSet<OrderEntity> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<OrderOwner> OrderOwners { get; set; } = null!;
    public DbSet<ProductCatalog> ProductCatalogs { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;
    public DbSet<InboxRetryHistory> InboxRetryHistories { get; set; } = null!;
    public DbSet<SagaExecutionRecordEntity> SagaExecutionRecords { get; set; } = null!;
}
