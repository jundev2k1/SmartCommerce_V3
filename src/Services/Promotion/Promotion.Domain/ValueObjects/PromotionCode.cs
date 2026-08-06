using System.Text.RegularExpressions;

namespace NovaCore.Promotion.Domain.ValueObjects;

/// <summary>Uppercase, human-assigned identifier for a Promotion (e.g. "SUMMER10OFF").</summary>
public sealed partial class PromotionCode : StringValueObject
{
    private const int MaxLength = 50;

    private PromotionCode(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out PromotionCode? code)
    {
        if (GetValidationError(value) is not null)
        {
            code = null;
            return false;
        }

        code = new PromotionCode(Normalize(value!));
        return true;
    }

    public static PromotionCode Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new PromotionCode(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Promotion code cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Promotion code cannot exceed {MaxLength} characters.");

        if (!CodeFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Promotion code must be uppercase alphanumeric with underscores/hyphens only.");

        return null;
    }

    [GeneratedRegex("^[A-Z0-9]+([_-][A-Z0-9]+)*$")]
    private static partial Regex CodeFormat();
}
