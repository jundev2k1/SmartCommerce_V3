namespace NovaCore.Promotion.Domain.ValueObjects;

/// <summary>
/// Shared start/end window used wherever an aggregate needs a reusable period shape (e.g.
/// CampaignSchedule.Period). Consolidated from 6 structurally-identical per-aggregate Period
/// Value Objects during the Phase 2.5 Domain Standardization Review - see
/// docs/promotion-service/value-objects/README.md. TimeZone is optional since two of the merged
/// aggregates (LoyaltyProgram, RecommendationProgram) had no TimeZone field to pair it with.
/// </summary>
public sealed class Period : ValueObject
{
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }
    public string? TimeZone { get; }

    private Period(DateTime startTime, DateTime endTime, string? timeZone)
    {
        StartTime = startTime;
        EndTime = endTime;
        TimeZone = timeZone;
    }

    public static Period Create(DateTime startTime, DateTime endTime, string? timeZone = null)
    {
        if (endTime <= startTime)
            throw ExceptionFactory.InvalidRange("End time must be after start time.");

        return new Period(startTime, endTime, timeZone?.Trim());
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartTime;
        yield return EndTime;
        yield return TimeZone ?? string.Empty;
    }
}
