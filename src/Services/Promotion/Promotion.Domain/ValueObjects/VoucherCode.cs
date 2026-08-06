using System.Text.RegularExpressions;

namespace NovaCore.Promotion.Domain.ValueObjects;

/// <summary>Uppercase redeemable code for a Voucher.</summary>
public sealed partial class VoucherCode : StringValueObject
{
    private const int MaxLength = 50;

    private VoucherCode(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out VoucherCode? code)
    {
        if (GetValidationError(value) is not null)
        {
            code = null;
            return false;
        }

        code = new VoucherCode(Normalize(value!));
        return true;
    }

    public static VoucherCode Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new VoucherCode(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Voucher code cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Voucher code cannot exceed {MaxLength} characters.");

        if (!CodeFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Voucher code must be uppercase alphanumeric with underscores/hyphens only.");

        return null;
    }

    [GeneratedRegex("^[A-Z0-9]+([_-][A-Z0-9]+)*$")]
    private static partial Regex CodeFormat();
}
