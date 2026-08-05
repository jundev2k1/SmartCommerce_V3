namespace NovaCore.BuildingBlock.Application.DeadLetters;

public enum DeadLetterRetryOutcome
{
    Succeeded,
    NotFound,
    NotDeadLetter,

    /// <summary>Another retry for the same row is already in flight (distributed lock not acquired).</summary>
    Conflict,

    /// <summary>Row was requeued but the republish to Kafka itself failed; reverted back to DeadLetter.</summary>
    PublishFailed,
}

public sealed record DeadLetterRetryAttemptResult(Guid InboxMessageId, DeadLetterRetryOutcome Outcome, string? Error);

/// <summary>
/// Owns the full admin-retry sequence for one dead-lettered Inbox row: acquire a per-row
/// distributed lock, requeue (atomic DB transition + history entry), republish through Kafka via
/// IOutboxPublisher (never re-invokes the consumer handler directly), and structured logging at
/// each step. Used identically by the single/bulk/retry-all commands and Carter endpoints - and,
/// by design, has no HTTP coupling, so a future scheduled-retry background job can reuse it
/// unchanged.
/// </summary>
public interface IDeadLetterRetryService
{
    Task<DeadLetterRetryAttemptResult> RetryAsync(Guid inboxMessageId, CancellationToken ct = default);
}
