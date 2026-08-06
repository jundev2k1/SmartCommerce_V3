namespace NovaCore.Promotion.Domain.ValueObjects;

/// <summary>Start/end window a Promotion is eligible within.</summary>
public sealed class PromotionPeriod : ValueObject
{
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }
    public string TimeZone { get; }

    private PromotionPeriod(DateTime startTime, DateTime endTime, string timeZone)
    {
        StartTime = startTime;
        EndTime = endTime;
        TimeZone = timeZone;
    }

    public static PromotionPeriod Create(DateTime startTime, DateTime endTime, string timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
            throw ExceptionFactory.RequiredField("Time zone cannot be empty.");

        if (endTime <= startTime)
            throw ExceptionFactory.InvalidRange("End time must be after start time.");

        return new PromotionPeriod(startTime, endTime, timeZone.Trim());
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartTime;
        yield return EndTime;
        yield return TimeZone;
    }
}
