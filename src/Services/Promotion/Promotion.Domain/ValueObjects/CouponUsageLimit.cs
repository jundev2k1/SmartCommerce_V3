namespace NovaCore.Promotion.Domain.ValueObjects;

/// <summary>Bundled usage-cap pair. Reserved - Coupon itself keeps plain MaxUsage/MaxUsagePerUser scalars per its literal Properties list; available for reuse wherever a bundled usage-limit shape is actually needed.</summary>
public sealed class CouponUsageLimit : ValueObject
{
    public int? MaxUsage { get; }
    public int? MaxUsagePerUser { get; }

    private CouponUsageLimit(int? maxUsage, int? maxUsagePerUser)
    {
        MaxUsage = maxUsage;
        MaxUsagePerUser = maxUsagePerUser;
    }

    public static CouponUsageLimit Create(int? maxUsage, int? maxUsagePerUser)
    {
        if (maxUsage is < 0)
            throw ExceptionFactory.InvalidRange("Max usage cannot be negative.");

        if (maxUsagePerUser is < 0)
            throw ExceptionFactory.InvalidRange("Max usage per user cannot be negative.");

        return new CouponUsageLimit(maxUsage, maxUsagePerUser);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return MaxUsage ?? -1;
        yield return MaxUsagePerUser ?? -1;
    }
}
