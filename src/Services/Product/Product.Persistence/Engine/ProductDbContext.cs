using BuildingBlock.Persistence.Ef.DbContext;
using BuildingBlock.Persistence.Ef.Inbox;
using BuildingBlock.Persistence.Ef.Outbox;

namespace Product.Persistence.Engine;

public sealed class ProductDbContext(DbContextOptions<ProductDbContext> options)
    : DbContextBase(options),
    IOutboxDbContext,
    IInboxDbContext
{
    public DbSet<ProductEntity> Products { get; set; } = null!;
    public DbSet<ProductVariation> ProductVariations { get; set; } = null!;
    public DbSet<ProductCategory> ProductCategories { get; set; } = null!;
    public DbSet<ProductTag> ProductTags { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;
}
