using System.Text.RegularExpressions;

namespace NovaCore.Promotion.Domain.ValueObjects;

/// <summary>
/// Shared uppercase, human-assigned identifier used by every aggregate root's own Code property
/// (Campaign, Promotion, Coupon, Voucher, LoyaltyProgram, RecommendationProgram, ProductSet).
/// Consolidated from 7 structurally-identical per-aggregate Code Value Objects during the Phase
/// 2.5 Domain Standardization Review - see docs/promotion-service/value-objects/README.md. Reusing
/// one shared VO across aggregates within the same Domain project does not compromise aggregate
/// independence (same reasoning already applies to the shared BuildingBlock Money/Quantity VOs);
/// it removed both the duplication and the CouponCode entity/VO name collision the original
/// per-aggregate CouponCode VO required an alias workaround for.
/// </summary>
public sealed partial class EntityCode : StringValueObject
{
    private const int MaxLength = 50;

    private EntityCode(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out EntityCode? code)
    {
        if (GetValidationError(value) is not null)
        {
            code = null;
            return false;
        }

        code = new EntityCode(Normalize(value!));
        return true;
    }

    public static EntityCode Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new EntityCode(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Code cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Code cannot exceed {MaxLength} characters.");

        if (!CodeFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Code must be uppercase alphanumeric with underscores/hyphens only.");

        return null;
    }

    [GeneratedRegex("^[A-Z0-9]+([_-][A-Z0-9]+)*$")]
    private static partial Regex CodeFormat();
}
