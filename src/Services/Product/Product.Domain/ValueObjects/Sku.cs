using SmartEcommerce.BuildingBlock.SharedKernel.Extensions;
using SmartEcommerce.BuildingBlock.SharedKernel.RegexPatterns;

namespace SmartEcommerce.Product.Domain.ValueObjects;

public sealed class Sku : StringValueObject
{
    private Sku(string val) : base(val) { }

    public static Sku Create(string val)
    {
        var normalizedVal = val?.Trim().ToUpperInvariant()
            ?? throw ExceptionFactory.InvalidRange("Product SKU is not valid.");

        if (!IsValid(normalizedVal))
            throw ExceptionFactory.InvalidRange("Product SKU is not valid.");

        return new Sku(normalizedVal);
    }

    public static bool IsValid(string val)
        => val.IsNotNullOrWhiteSpace()
            && val.Length >= 3
            && val.Length <= 30
            && RegexPatterns.Sku().IsMatch(val);
}
