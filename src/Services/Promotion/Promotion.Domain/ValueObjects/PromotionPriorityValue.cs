namespace NovaCore.Promotion.Domain.ValueObjects;

/// <summary>Validated numeric priority weight (0-100, higher wins) used by PromotionPriority - distinct from the plain ordering int on Campaign/Promotion.</summary>
public sealed class PromotionPriorityValue : ValueObject
{
    public int Value { get; }

    private PromotionPriorityValue(int value)
    {
        Value = value;
    }

    public static PromotionPriorityValue Create(int value)
    {
        if (value < 0 || value > 100)
            throw ExceptionFactory.InvalidRange("Promotion priority value must be between 0 and 100.");

        return new PromotionPriorityValue(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
