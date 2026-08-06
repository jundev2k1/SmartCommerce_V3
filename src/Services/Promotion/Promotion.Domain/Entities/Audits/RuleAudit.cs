namespace NovaCore.Promotion.Domain.Entities.Audits;

/// <summary>An audit row scoped to a rule-shaped entity (RecommendationRule, PointRule, PromotionRule, ...) - CreatedAt is inherited from BaseEntity, not redeclared.</summary>
public sealed class RuleAudit : BaseEntity<Guid>, IAuditable
{
    public Guid RuleId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public Guid? OperatorId { get; private set; }

    private RuleAudit() { }

    public static RuleAudit Create(Guid ruleId, string action, Guid? operatorId = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw ExceptionFactory.RequiredField("Audit action cannot be empty.");

        return new RuleAudit
        {
            Id = Guid.CreateVersion7(),
            RuleId = ruleId,
            Action = action,
            OperatorId = operatorId,
        };
    }
}
