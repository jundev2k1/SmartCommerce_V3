namespace NovaCore.Promotion.Domain.Entities.Distributions;

/// <summary>A retry counter for a DistributionExecution - not navigated from DistributionJob, so construction is public. No retry-scheduling/backoff logic lives here.</summary>
public sealed class DistributionRetry : BaseEntity<Guid>, IAuditable
{
    public Guid ExecutionId { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? LastRetryAt { get; private set; }

    private DistributionRetry() { }

    public static DistributionRetry Create(Guid executionId)
    {
        return new DistributionRetry
        {
            Id = Guid.CreateVersion7(),
            ExecutionId = executionId,
        };
    }

    /// <summary>Structural counter increment only - no backoff/scheduling logic lives here.</summary>
    public void RecordRetry()
    {
        RetryCount++;
        LastRetryAt = DateTime.UtcNow;
    }
}
