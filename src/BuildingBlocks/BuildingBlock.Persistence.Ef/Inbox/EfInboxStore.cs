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

    public async Task<InboxAttemptDecision> BeginAttemptAsync(
        Guid messageId,
        string consumerName,
        string topic,
        string payload,
        string headersJson,
        CancellationToken ct = default)
    {
        var existing = await _context.InboxMessages
            .FirstOrDefaultAsync(x => x.MessageId == messageId && x.ConsumerName == consumerName, ct);
        if (existing is not null)
        {
            return existing.Status switch
            {
                InboxMessageStatus.Processed => InboxAttemptDecision.AlreadyProcessed,
                InboxMessageStatus.DeadLetter => InboxAttemptDecision.DeadLettered,
                InboxMessageStatus.Retrying when existing.NextRetryAt > DateTime.UtcNow
                    => InboxAttemptDecision.NotDueYet,
                _ => InboxAttemptDecision.Proceed
            };
        }

        var inbox = InboxMessage.Create(
            messageId,
            consumerName,
            topic,
            payload,
            headersJson);

        await _context.InboxMessages.AddAsync(inbox, ct);

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return InboxAttemptDecision.AlreadyProcessed;
        }

        return InboxAttemptDecision.Proceed;
    }

    public async Task CompleteAttemptAsync(
        Guid messageId,
        string consumerName,
        CancellationToken ct = default)
    {
        var inbox = await _context.InboxMessages
            .FirstOrDefaultAsync(x => x.MessageId == messageId && x.ConsumerName == consumerName, ct);
        if (inbox is null)
            return;

        inbox.MarkProcessed();
        await _context.SaveChangesAsync(ct);
    }

    public async Task<InboxFailureOutcome> FailAttemptAsync(
        Guid messageId,
        string consumerName,
        string error,
        InboxRetryPolicy policy,
        CancellationToken ct = default)
    {
        var inbox = await _context.InboxMessages
            .FirstOrDefaultAsync(x => x.MessageId == messageId && x.ConsumerName == consumerName, ct);
        if (inbox is null)
            return InboxFailureOutcome.WillRetry;

        inbox.MarkFailed(error, policy);

        await _context.SaveChangesAsync(ct);

        return inbox.Status == InboxMessageStatus.DeadLetter
            ? InboxFailureOutcome.DeadLettered
            : InboxFailureOutcome.WillRetry;
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

    public async Task<IReadOnlyList<InboxDeadLetterSummary>> GetDeadLetterSummaryAsync(CancellationToken ct = default)
    {
        var groups = await _context.InboxMessages
            .AsNoTracking()
            .Where(m => m.Status == InboxMessageStatus.DeadLetter)
            .GroupBy(m => new { m.ConsumerName, m.Topic })
            .Select(g => new
            {
                g.Key.ConsumerName,
                g.Key.Topic,
                Count = g.Count(),
                OldestDeadLetteredAt = g.Min(m => m.LastRetryAt)
            })
            .ToListAsync(ct);

        return [.. groups.Select(g => new InboxDeadLetterSummary(
            g.ConsumerName, g.Topic, g.Count, g.OldestDeadLetteredAt ?? DateTime.UtcNow))];
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
