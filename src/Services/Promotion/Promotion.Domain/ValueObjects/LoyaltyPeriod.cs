namespace NovaCore.Promotion.Domain.ValueObjects;

/// <summary>Start/end window a LoyaltyProgram runs within. Reserved - LoyaltyProgram itself keeps plain StartTime/EndTime scalars per its literal Properties list (it also has no TimeZone property, unlike Campaign/Promotion/Coupon/Voucher - this VO keeps TimeZone for shape-consistency with the other *Period VOs, but nothing consumes it yet).</summary>
public sealed class LoyaltyPeriod : ValueObject
{
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }
    public string TimeZone { get; }

    private LoyaltyPeriod(DateTime startTime, DateTime endTime, string timeZone)
    {
        StartTime = startTime;
        EndTime = endTime;
        TimeZone = timeZone;
    }

    public static LoyaltyPeriod Create(DateTime startTime, DateTime endTime, string timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
            throw ExceptionFactory.RequiredField("Time zone cannot be empty.");

        if (endTime <= startTime)
            throw ExceptionFactory.InvalidRange("End time must be after start time.");

        return new LoyaltyPeriod(startTime, endTime, timeZone.Trim());
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartTime;
        yield return EndTime;
        yield return TimeZone;
    }
}
