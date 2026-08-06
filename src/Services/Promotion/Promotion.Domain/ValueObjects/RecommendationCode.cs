using System.Text.RegularExpressions;

namespace NovaCore.Promotion.Domain.ValueObjects;

/// <summary>Uppercase, human-assigned identifier for a RecommendationProgram.</summary>
public sealed partial class RecommendationCode : StringValueObject
{
    private const int MaxLength = 50;

    private RecommendationCode(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out RecommendationCode? code)
    {
        if (GetValidationError(value) is not null)
        {
            code = null;
            return false;
        }

        code = new RecommendationCode(Normalize(value!));
        return true;
    }

    public static RecommendationCode Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new RecommendationCode(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Recommendation code cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Recommendation code cannot exceed {MaxLength} characters.");

        if (!CodeFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Recommendation code must be uppercase alphanumeric with underscores/hyphens only.");

        return null;
    }

    [GeneratedRegex("^[A-Z0-9]+([_-][A-Z0-9]+)*$")]
    private static partial Regex CodeFormat();
}
