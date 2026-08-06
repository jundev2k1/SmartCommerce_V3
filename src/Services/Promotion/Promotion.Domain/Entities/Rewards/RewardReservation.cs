namespace NovaCore.Promotion.Domain.Entities.Rewards;

/// <summary>A temporary hold on an issued reward - RewardId is a loose reference (no dedicated "Reward" entity exists in this pass). Not navigated from RewardProgram, so construction is public. No expiry sweeping logic lives here.</summary>
public sealed class RewardReservation : BaseEntity<Guid>, IAuditable
{
    public Guid RewardId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime ReservedAt { get; private set; }
    public DateTime? ExpiredAt { get; private set; }

    private RewardReservation() { }

    public static RewardReservation Create(Guid rewardId, Guid userId, DateTime? expiredAt = null)
    {
        return new RewardReservation
        {
            Id = Guid.CreateVersion7(),
            RewardId = rewardId,
            UserId = userId,
            ReservedAt = DateTime.UtcNow,
            ExpiredAt = expiredAt,
        };
    }
}
