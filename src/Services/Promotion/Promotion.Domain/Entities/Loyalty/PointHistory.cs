namespace NovaCore.Promotion.Domain.Entities.Loyalty;

/// <summary>An append-only audit trail row for a PointAccount - CreatedAt is inherited from BaseEntity, not redeclared. Not navigated from PointAccount, so construction is public.</summary>
public sealed class PointHistory : BaseEntity<Guid>, IAuditable
{
    public Guid AccountId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public Guid? OperatorId { get; private set; }

    private PointHistory() { }

    public static PointHistory Create(Guid accountId, string action, Guid? operatorId)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw ExceptionFactory.RequiredField("History action cannot be empty.");

        return new PointHistory
        {
            Id = Guid.CreateVersion7(),
            AccountId = accountId,
            Action = action,
            OperatorId = operatorId,
        };
    }
}
