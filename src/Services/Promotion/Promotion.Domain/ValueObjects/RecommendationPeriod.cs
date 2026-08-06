namespace NovaCore.Promotion.Domain.ValueObjects;

/// <summary>Start/end window a RecommendationProgram runs within. Reserved - RecommendationProgram itself keeps plain StartTime/EndTime scalars per its literal Properties list (no TimeZone property either, same as LoyaltyProgram).</summary>
public sealed class RecommendationPeriod : ValueObject
{
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }

    private RecommendationPeriod(DateTime startTime, DateTime endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
    }

    public static RecommendationPeriod Create(DateTime startTime, DateTime endTime)
    {
        if (endTime <= startTime)
            throw ExceptionFactory.InvalidRange("End time must be after start time.");

        return new RecommendationPeriod(startTime, endTime);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartTime;
        yield return EndTime;
    }
}
