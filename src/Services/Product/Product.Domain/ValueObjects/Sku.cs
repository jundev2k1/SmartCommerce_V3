using System.Text.RegularExpressions;

namespace Product.Domain.ValueObjects;

/// <summary>Stock keeping unit - identifies a single ProductVariation. Not shared across variations.</summary>
public sealed partial class Sku : StringValueObject
{
    private const int MaxLength = 50;

    private Sku(string value) : base(value) { }

    /// <summary>Cheap pre-check for upper layers (e.g. FluentValidation) - shares GetValidationError with Create so the two never diverge.</summary>
    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out Sku? sku)
    {
        if (GetValidationError(value) is not null)
        {
            sku = null;
            return false;
        }

        sku = new Sku(Normalize(value!));
        return true;
    }

    public static Sku Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new Sku(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("SKU cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"SKU cannot exceed {MaxLength} characters.");

        if (!SkuFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("SKU may only contain letters, digits, and hyphens.");

        return null;
    }

    [GeneratedRegex("^[A-Z0-9-]+$")]
    private static partial Regex SkuFormat();
}
