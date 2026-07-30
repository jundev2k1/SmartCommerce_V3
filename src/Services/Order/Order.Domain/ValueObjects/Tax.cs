namespace Order.Domain.ValueObjects;

public sealed class Tax : ValueObject
{
    /// <summary>Defines how the tax value should be interpreted.</summary>
    public TaxMethod Method { get; }

    /// <summary>
    /// Configured tax value.
    ///
    /// FixedAmount:
    ///     Monetary amount.
    ///
    /// Percentage:
    ///     Percentage value (0 - 100).
    /// </summary>
    public decimal Value { get; }

    /// <summary>Actual tax amount applied to the order.</summary>
    public Money AppliedAmount { get; }

    private Tax(
        TaxMethod method,
        decimal value,
        Money appliedAmount)
    {
        Method = method;
        Value = value;
        AppliedAmount = appliedAmount;
    }

    public static Tax FixedAmount(
        Money amount)
    {
        ArgumentNullException.ThrowIfNull(amount);

        return new Tax(
            TaxMethod.FixedAmount,
            amount.Value,
            amount);
    }

    public static Tax Percentage(
        decimal percentage,
        Money appliedAmount)
    {
        if (!IsValidPercentage(percentage))
            throw ExceptionFactory.InvalidRange("Tax percentage must be between 0 and 100.");

        ArgumentNullException.ThrowIfNull(appliedAmount);

        return new Tax(
            TaxMethod.Percentage,
            percentage,
            appliedAmount);
    }

    public Money GetFixedAmount()
    {
        if (Method != TaxMethod.FixedAmount)
            throw ExceptionFactory.InvalidState("Tax is not a fixed amount.");

        return Money.Create(Value);
    }

    public decimal GetPercentage()
    {
        if (Method != TaxMethod.Percentage)
            throw ExceptionFactory.InvalidState("Tax is not a percentage.");

        return Value;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Method;
        yield return Value;
        yield return AppliedAmount;
    }

    public static bool IsValidPercentage(decimal percentage)
    {
        return percentage >= 0m && percentage <= 100m;
    }
}
