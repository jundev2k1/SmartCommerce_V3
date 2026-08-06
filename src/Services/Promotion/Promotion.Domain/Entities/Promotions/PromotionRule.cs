namespace NovaCore.Promotion.Domain.Entities.Promotions;

/// <summary>A named eligibility rule under a Promotion, optionally grouped by PromotionRuleGroup. No condition evaluation lives here - see PromotionCondition for the structural condition rows.</summary>
public sealed class PromotionRule : BaseEntity<Guid>, IAuditable
{
    public Guid PromotionId { get; private set; }
    public Guid? RuleGroupId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsEnabled { get; private set; }

    private PromotionRule() { }

    /// <summary>Only Promotion may construct a PromotionRule - see Promotion.AddRule.</summary>
    internal static PromotionRule Create(Guid promotionId, Guid? ruleGroupId, string name, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Rule name cannot be empty.");

        return new PromotionRule
        {
            Id = Guid.CreateVersion7(),
            PromotionId = promotionId,
            RuleGroupId = ruleGroupId,
            Name = name,
            DisplayOrder = displayOrder,
            IsEnabled = true,
        };
    }

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;
}
