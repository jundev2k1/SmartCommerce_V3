using SmartEcommerce.BuildingBlock.Persistence.Ef.DbContext;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Inbox;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Outbox;

namespace SmartEcommerce.User.Persistence.Engine;

public sealed class UserDbContext(DbContextOptions<UserDbContext> options)
    : DbContextBase(options),
    IOutboxDbContext,
    IInboxDbContext
{
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;
    public DbSet<InboxRetryHistory> InboxRetryHistories { get; set; } = null!;
}
