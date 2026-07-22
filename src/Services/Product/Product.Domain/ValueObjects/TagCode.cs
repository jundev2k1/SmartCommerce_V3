using System.Text.RegularExpressions;

namespace Product.Domain.ValueObjects;

/// <summary>Unique business code identifying a ProductTag.</summary>
public sealed partial class TagCode : StringValueObject
{
    private const int MaxLength = 50;

    private TagCode(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out TagCode? code)
    {
        if (GetValidationError(value) is not null)
        {
            code = null;
            return false;
        }

        code = new TagCode(Normalize(value!));
        return true;
    }

    public static TagCode Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new TagCode(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Tag code cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Tag code cannot exceed {MaxLength} characters.");

        if (!CodeFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Tag code may only contain letters, digits, and hyphens.");

        return null;
    }

    [GeneratedRegex("^[A-Z0-9-]+$")]
    private static partial Regex CodeFormat();
}
