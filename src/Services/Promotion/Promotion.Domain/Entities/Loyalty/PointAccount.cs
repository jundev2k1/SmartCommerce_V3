namespace NovaCore.Promotion.Domain.Entities.Loyalty;

/// <summary>A user's point balance under a LoyaltyProgram - no earn/spend calculation lives here, see UpdateBalances.</summary>
public sealed class PointAccount : BaseEntity<Guid>, IAuditable
{
    public Guid UserId { get; private set; }
    public Guid ProgramId { get; private set; }
    public int AvailablePoints { get; private set; }
    public int PendingPoints { get; private set; }
    public int ExpiredPoints { get; private set; }
    public int LifetimeEarned { get; private set; }
    public int LifetimeSpent { get; private set; }

    private PointAccount() { }

    /// <summary>Only LoyaltyProgram may construct a PointAccount - see LoyaltyProgram.AddAccount.</summary>
    internal static PointAccount Create(Guid programId, Guid userId)
    {
        return new PointAccount
        {
            Id = Guid.CreateVersion7(),
            ProgramId = programId,
            UserId = userId,
        };
    }

    /// <summary>Structural balance setters only - no earn/spend/expiry calculation lives here.</summary>
    public void UpdateBalances(int availablePoints, int pendingPoints, int expiredPoints, int lifetimeEarned, int lifetimeSpent)
    {
        AvailablePoints = availablePoints;
        PendingPoints = pendingPoints;
        ExpiredPoints = expiredPoints;
        LifetimeEarned = lifetimeEarned;
        LifetimeSpent = lifetimeSpent;
    }
}
