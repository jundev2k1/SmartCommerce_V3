namespace NovaCore.Promotion.Domain.Entities.Rewards;

/// <summary>An append-only audit trail row for an issued reward - CreatedAt is inherited from BaseEntity, not redeclared. Not navigated from RewardProgram, so construction is public.</summary>
public sealed class RewardHistory : BaseEntity<Guid>, IAuditable
{
    public Guid RewardId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public Guid? OperatorId { get; private set; }

    private RewardHistory() { }

    public static RewardHistory Create(Guid rewardId, string action, Guid? operatorId)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw ExceptionFactory.RequiredField("History action cannot be empty.");

        return new RewardHistory
        {
            Id = Guid.CreateVersion7(),
            RewardId = rewardId,
            Action = action,
            OperatorId = operatorId,
        };
    }
}
