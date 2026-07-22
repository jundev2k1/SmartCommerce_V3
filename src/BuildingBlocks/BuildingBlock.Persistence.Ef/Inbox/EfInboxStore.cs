using BuildingBlock.Persistence.Inbox;

using Microsoft.EntityFrameworkCore;

namespace BuildingBlock.Persistence.Ef.Inbox;

/// <summary>
/// Generic EF implementation of IInboxStore, parameterized over the DbContext type.
/// Derived DbContexts must implement IInboxDbContext to provide access to InboxMessages.
///
/// Scoped lifetime (one instance per Kafka message / per DI scope), same as the DbContext it
/// wraps. BeginAttemptAsync stages the row it looked up (or created) on this instance so the
/// matching CompleteAttemptAsync/FailAttemptAsync call - later in the same scope - can finish
/// mutating that exact tracked entity instead of re-querying it.
/// </summary>
public sealed class EfInboxStore<TContext>(TContext context) : IInboxStore
    where TContext : Microsoft.EntityFrameworkCore.DbContext, IInboxDbContext
{
    private readonly TContext _context = context;
    private InboxMessage? _currentAttempt;

    public async Task<InboxAttemptDecision> BeginAttemptAsync(
        Guid messageId,
        string consumerName,
        string topic,
        string payload,
        string headersJson,
        CancellationToken ct = default)
    {
        var existing = await _context.InboxMessages
            .FirstOrDefaultAsync(m => m.MessageId == messageId && m.ConsumerName == consumerName, ct);

        if (existing is not null)
        {
            switch (existing.Status)
            {
                case InboxMessageStatus.Processed:
                    return InboxAttemptDecision.AlreadyProcessed;
                case InboxMessageStatus.DeadLetter:
                    return InboxAttemptDecision.DeadLettered;
                case InboxMessageStatus.Retrying when existing.NextRetryAt > DateTime.UtcNow:
                    return InboxAttemptDecision.NotDueYet;
            }

            existing.RefreshAttemptContext(topic, payload, headersJson);
            existing.MarkProcessed();
            _currentAttempt = existing;
            return InboxAttemptDecision.Proceed;
        }

        var created = InboxMessage.Create(messageId, consumerName, topic, payload, headersJson);
        created.MarkProcessed();
        await _context.InboxMessages.AddAsync(created, ct);

        _currentAttempt = created;
        return InboxAttemptDecision.Proceed;
    }

    public async Task CompleteAttemptAsync(CancellationToken ct = default)
    {
        if (_currentAttempt is null)
            return;

        // No-op if the handler's own SaveChanges already flushed this entity (its tracker state
        // is already Unchanged) - this call only matters for handlers that never persist anything
        // themselves.
        await _context.SaveChangesAsync(ct);
        _currentAttempt = null;
    }

    public async Task<InboxFailureOutcome> FailAttemptAsync(
        string error,
        InboxRetryPolicy policy,
        CancellationToken ct = default)
    {
        if (_currentAttempt is null)
            return InboxFailureOutcome.WillRetry;

        var entry = _context.Entry(_currentAttempt);

        if (entry.State == EntityState.Unchanged)
        {
            // The optimistic Processed marker staged in BeginAttemptAsync was already flushed and
            // committed by the handler's own SaveChanges before it threw downstream. The business
            // change (and this marker, in the same transaction) already happened - flipping the
            // row back to Retrying now would cause the next attempt to re-run already-committed
            // business logic, which is exactly the duplicate-side-effect bug this is meant to
            // prevent. Leave it Processed.
            _currentAttempt = null;
            return InboxFailureOutcome.AlreadyCommitted;
        }

        if (entry.State == EntityState.Detached)
        {
            // Rolled back (EfUnitOfWork.ExecuteTransactionAsync clears the tracker on rollback) -
            // nothing was committed. Re-add so the failure state below can still be persisted.
            _context.InboxMessages.Add(_currentAttempt);
        }

        _currentAttempt.MarkFailed(error, policy);
        await _context.SaveChangesAsync(ct);

        var outcome = _currentAttempt.Status == InboxMessageStatus.DeadLetter
            ? InboxFailureOutcome.DeadLettered
            : InboxFailureOutcome.WillRetry;

        _currentAttempt = null;
        return outcome;
    }

    public async Task<IReadOnlyList<InboxMessageSnapshot>> GetDueForRetryAsync(int batchSize, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var messages = await _context.InboxMessages
            .AsNoTracking()
            .Where(m => m.Status == InboxMessageStatus.Retrying && m.NextRetryAt <= now)
            .OrderBy(m => m.NextRetryAt)
            .Take(batchSize)
            .ToListAsync(ct);

        return [.. messages.Select(ToSnapshot)];
    }

    public async Task<int> DeleteProcessedBeforeAsync(DateTime olderThanUtc, int batchSize, CancellationToken ct = default)
    {
        var ids = await _context.InboxMessages
            .Where(m => m.Status == InboxMessageStatus.Processed && m.ProcessedAt < olderThanUtc)
            .OrderBy(m => m.ProcessedAt)
            .Take(batchSize)
            .Select(m => m.Id)
            .ToListAsync(ct);

        if (ids.Count == 0)
            return 0;

        await _context.InboxMessages
            .Where(m => ids.Contains(m.Id))
            .ExecuteDeleteAsync(ct);

        return ids.Count;
    }

    private static InboxMessageSnapshot ToSnapshot(InboxMessage m) => new(
        m.MessageId,
        m.ConsumerName,
        m.Topic,
        m.Payload,
        m.HeadersJson,
        m.Status,
        m.RetryCount,
        m.CreatedAt,
        m.ProcessedAt,
        m.NextRetryAt,
        m.LastRetryAt,
        m.LastError);
}
