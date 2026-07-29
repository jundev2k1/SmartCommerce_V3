using BuildingBlock.Persistence.Ef.DbContext;
using BuildingBlock.Persistence.Ef.Inbox;
using BuildingBlock.Persistence.Ef.Outbox;

namespace User.Persistence.Engine;

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
