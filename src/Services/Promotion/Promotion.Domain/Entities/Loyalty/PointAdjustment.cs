namespace NovaCore.Promotion.Domain.Entities.Loyalty;

/// <summary>A manual point correction for a PointAccount - not navigated from PointAccount, so construction is public. No approval/validation logic lives here.</summary>
public sealed class PointAdjustment : BaseEntity<Guid>, IAuditable
{
    public Guid AccountId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public int Points { get; private set; }
    public Guid? OperatorId { get; private set; }
    public DateTime AdjustedAt { get; private set; }

    private PointAdjustment() { }

    public static PointAdjustment Create(Guid accountId, string reason, int points, Guid? operatorId)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw ExceptionFactory.RequiredField("Adjustment reason cannot be empty.");

        return new PointAdjustment
        {
            Id = Guid.CreateVersion7(),
            AccountId = accountId,
            Reason = reason,
            Points = points,
            OperatorId = operatorId,
            AdjustedAt = DateTime.UtcNow,
        };
    }
}
