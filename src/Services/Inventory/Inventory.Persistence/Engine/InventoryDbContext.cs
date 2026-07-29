using BuildingBlock.Persistence.Ef.DbContext;
using BuildingBlock.Persistence.Ef.Inbox;
using BuildingBlock.Persistence.Ef.Outbox;

namespace Inventory.Persistence.Engine;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options)
    : DbContextBase(options),
    IInboxDbContext,
    IOutboxDbContext
{
    public DbSet<InventoryEntity> Inventories { get; set; } = null!;
    public DbSet<Warehouse> Warehouses { get; set; } = null!;
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; } = null!;
    public DbSet<StockDeduction> StockDeductions { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;
    public DbSet<InboxRetryHistory> InboxRetryHistories { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
}
