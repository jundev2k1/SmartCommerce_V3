namespace NovaCore.Promotion.Domain.ValueObjects;

/// <summary>Start/end window a Coupon is redeemable within. Reserved - Coupon itself keeps plain StartTime/EndTime/TimeZone scalars per its literal Properties list (same reconciliation as CampaignPeriod/PromotionPeriod).</summary>
public sealed class CouponPeriod : ValueObject
{
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }
    public string TimeZone { get; }

    private CouponPeriod(DateTime startTime, DateTime endTime, string timeZone)
    {
        StartTime = startTime;
        EndTime = endTime;
        TimeZone = timeZone;
    }

    public static CouponPeriod Create(DateTime startTime, DateTime endTime, string timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
            throw ExceptionFactory.RequiredField("Time zone cannot be empty.");

        if (endTime <= startTime)
            throw ExceptionFactory.InvalidRange("End time must be after start time.");

        return new CouponPeriod(startTime, endTime, timeZone.Trim());
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartTime;
        yield return EndTime;
        yield return TimeZone;
    }
}
