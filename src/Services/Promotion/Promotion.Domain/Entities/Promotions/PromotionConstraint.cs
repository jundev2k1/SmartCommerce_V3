namespace NovaCore.Promotion.Domain.Entities.Promotions;

/// <summary>A structural (type, value) restriction on a Promotion (e.g. minimum order amount) - no validation/enforcement logic lives here.</summary>
public sealed class PromotionConstraint : BaseEntity<Guid>, IAuditable
{
    public Guid PromotionId { get; private set; }
    public string ConstraintType { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;

    private PromotionConstraint() { }

    /// <summary>Only Promotion may construct a PromotionConstraint - see Promotion.AddConstraint.</summary>
    internal static PromotionConstraint Create(Guid promotionId, string constraintType, string value)
    {
        if (string.IsNullOrWhiteSpace(constraintType))
            throw ExceptionFactory.RequiredField("Constraint type cannot be empty.");

        return new PromotionConstraint
        {
            Id = Guid.CreateVersion7(),
            PromotionId = promotionId,
            ConstraintType = constraintType,
            Value = value,
        };
    }
}
