using NovaCore.BuildingBlock.Persistence.Ef.DbContext;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;

namespace NovaCore.Promotion.Persistence.Engine;

public sealed class PromotionDbContext(DbContextOptions<PromotionDbContext> options)
    : DbContextBase(options),
    IOutboxDbContext,
    IInboxDbContext
{
    // No aggregate DbSets yet - added in Phase 3 (Persistence) once Phase 2 (Domain Model)
    // supplies entities to map. See docs/promotion-service/phases/phase-3-persistence.md.

    // Reliability
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;
    public DbSet<InboxRetryHistory> InboxRetryHistories { get; set; } = null!;
}
