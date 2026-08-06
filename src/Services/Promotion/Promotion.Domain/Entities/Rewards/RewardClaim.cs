namespace NovaCore.Promotion.Domain.Entities.Rewards;

/// <summary>A record of a user claiming an issued reward - RewardId is a loose reference (no dedicated "Reward" entity exists in this pass). Not navigated from RewardProgram, so construction is public.</summary>
public sealed class RewardClaim : BaseEntity<Guid>, IAuditable
{
    public Guid RewardId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime ClaimedAt { get; private set; }

    private RewardClaim() { }

    public static RewardClaim Create(Guid rewardId, Guid userId)
    {
        return new RewardClaim
        {
            Id = Guid.CreateVersion7(),
            RewardId = rewardId,
            UserId = userId,
            ClaimedAt = DateTime.UtcNow,
        };
    }
}
