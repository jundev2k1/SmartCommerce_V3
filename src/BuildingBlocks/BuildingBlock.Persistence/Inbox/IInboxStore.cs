namespace BuildingBlock.Persistence.Inbox;

/// <summary>
/// Outcome of <see cref="IInboxStore.FailAttemptAsync"/>.
/// </summary>
public enum InboxFailureOutcome
{
    /// <summary>
    /// The consumer's own persistence already committed - and the optimistic Processed marker
    /// staged by BeginAttemptAsync committed in that same transaction - before the exception was
    /// observed here. The row is left Processed: flipping it back to Retrying would risk running
    /// already-committed business logic again on the next attempt.
    /// </summary>
    AlreadyCommitted,

    /// <summary>Nothing committed. RetryCount incremented, row left Retrying with a new NextRetryAt.</summary>
    WillRetry,

    /// <summary>Nothing committed. RetryCount exceeded MaxRetryCount, row moved to DeadLetter.</summary>
    DeadLettered,
}

public interface IInboxStore
{
    /// <summary>
    /// Looks up the (messageId, consumerName) row and decides whether the caller should invoke
    /// the consumer handler. When the decision is Proceed, the row is created (if absent) or
    /// reused, Topic/Payload/Headers are (re)captured for a possible future retry, and the row is
    /// optimistically staged as Processed on the current unit of work's change tracker - NOT
    /// saved. If the caller's own persistence (invoked between this call and
    /// CompleteAttemptAsync/FailAttemptAsync) issues its own SaveChanges, that same call commits
    /// the Processed marker atomically with the business change. CompleteAttemptAsync/
    /// FailAttemptAsync must be called afterward to finalize (or correct) the outcome.
    /// </summary>
    Task<InboxAttemptDecision> BeginAttemptAsync(
        Guid messageId,
        string consumerName,
        string topic,
        string payload,
        string headersJson,
        CancellationToken ct = default);

    /// <summary>
    /// Call after the handler invoked following a Proceed decision completes successfully.
    /// Flushes the optimistic Processed marker if it wasn't already committed by the handler's
    /// own SaveChanges.
    /// </summary>
    Task CompleteAttemptAsync(CancellationToken ct = default);

    /// <summary>
    /// Call after the handler invoked following a Proceed decision throws. See
    /// <see cref="InboxFailureOutcome"/> for the three possible outcomes.
    /// </summary>
    Task<InboxFailureOutcome> FailAttemptAsync(
        string error,
        InboxRetryPolicy policy,
        CancellationToken ct = default);

    /// <summary>Rows currently Retrying whose NextRetryAt has arrived, oldest first.</summary>
    Task<IReadOnlyList<InboxMessageSnapshot>> GetDueForRetryAsync(int batchSize, CancellationToken ct = default);

    /// <summary>
    /// Delete one batch of Processed rows whose ProcessedAt is older than <paramref name="olderThanUtc"/>.
    /// Never touches Pending/Retrying/DeadLetter rows. Returns the number of rows deleted, so the
    /// caller can loop until a batch comes back short.
    /// </summary>
    Task<int> DeleteProcessedBeforeAsync(DateTime olderThanUtc, int batchSize, CancellationToken ct = default);
}
