using System.Text.RegularExpressions;

namespace Product.Domain.ValueObjects;

/// <summary>Shared style/model code for a Product, distinct from each variation's own Sku.</summary>
public sealed partial class ProductCode : StringValueObject
{
    private const int MaxLength = 50;

    private ProductCode(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out ProductCode? code)
    {
        if (GetValidationError(value) is not null)
        {
            code = null;
            return false;
        }

        code = new ProductCode(Normalize(value!));
        return true;
    }

    public static ProductCode Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new ProductCode(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Product code cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Product code cannot exceed {MaxLength} characters.");

        if (!CodeFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Product code may only contain letters, digits, and hyphens.");

        return null;
    }

    [GeneratedRegex("^[A-Z0-9-]+$")]
    private static partial Regex CodeFormat();
}
