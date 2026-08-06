using System.Text.RegularExpressions;

namespace NovaCore.Promotion.Domain.ValueObjects;

/// <summary>Uppercase, human-assigned identifier for a LoyaltyProgram (e.g. "VIP_TIER_2026").</summary>
public sealed partial class LoyaltyProgramCode : StringValueObject
{
    private const int MaxLength = 50;

    private LoyaltyProgramCode(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out LoyaltyProgramCode? code)
    {
        if (GetValidationError(value) is not null)
        {
            code = null;
            return false;
        }

        code = new LoyaltyProgramCode(Normalize(value!));
        return true;
    }

    public static LoyaltyProgramCode Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new LoyaltyProgramCode(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Loyalty program code cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Loyalty program code cannot exceed {MaxLength} characters.");

        if (!CodeFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Loyalty program code must be uppercase alphanumeric with underscores/hyphens only.");

        return null;
    }

    [GeneratedRegex("^[A-Z0-9]+([_-][A-Z0-9]+)*$")]
    private static partial Regex CodeFormat();
}
