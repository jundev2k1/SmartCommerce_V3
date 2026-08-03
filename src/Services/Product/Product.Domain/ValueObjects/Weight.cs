namespace SmartEcommerce.Product.Domain.ValueObjects;

public sealed class Weight : ValueObject
{
    public decimal Value { get; }
    public WeightUnit Unit { get; }

    private Weight(decimal value, WeightUnit unit)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Weight cannot be negative.");

        Value = value;
        Unit = unit;
    }

    public static Weight Create(decimal value, WeightUnit unit)
        => new(value, unit);

    public static Weight FromKilograms(decimal value)
        => new(value, WeightUnit.Kilogram);

    public static Weight FromGrams(decimal value)
        => new(value, WeightUnit.Gram);

    public decimal ToKilograms() =>
        Unit switch
        {
            WeightUnit.Kilogram => Value,
            WeightUnit.Gram => Value / 1000m,
            _ => throw new NotSupportedException($"Unsupported unit: {Unit}")
        };

    public decimal ToGrams() =>
        Unit switch
        {
            WeightUnit.Kilogram => Value * 1000m,
            WeightUnit.Gram => Value,
            _ => throw new NotSupportedException($"Unsupported unit: {Unit}")
        };

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Unit;
    }

    public override string ToString() => $"{Value} {Unit}";
}
