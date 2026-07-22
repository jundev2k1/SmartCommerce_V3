using BuildingBlock.Persistence.Inbox;
using BuildingBlock.Persistence.Mongo.MongoContext;

using MongoDB.Driver;

namespace BuildingBlock.Persistence.Mongo.Inbox;

/// <summary>
/// Generic Mongo implementation of IInboxStore, parameterized over the Mongo context type.
/// Derived contexts must implement IInboxMongoContext to provide access to InboxMessages.
///
/// Unlike EfInboxStore, there is no shared change tracker to stage an optimistic completion
/// marker into: Audit.Persistence.UnitOfWork already documents that Mongo writes commit
/// immediately per call, with no SaveChanges to flush. CompleteAttemptAsync therefore writes the
/// Processed status directly once the handler returns, the same small window that existed before
/// this change - InboxFailureOutcome.AlreadyCommitted (the EF provider's atomic-commit detection)
/// never applies here.
/// </summary>
public sealed class MongoInboxStore<TContext>(TContext context) : IInboxStore
    where TContext : MongoContextBase, IInboxMongoContext
{
    private readonly TContext _context = context;
    private Guid? _currentAttemptId;

    public async Task<InboxAttemptDecision> BeginAttemptAsync(
        Guid messageId,
        string consumerName,
        string topic,
        string payload,
        string headersJson,
        CancellationToken ct = default)
    {
        var existing = await _context.InboxMessages
            .Find(m => m.MessageId == messageId && m.ConsumerName == consumerName)
            .FirstOrDefaultAsync(ct);

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

            _currentAttemptId = existing.Id;
            return InboxAttemptDecision.Proceed;
        }

        var created = InboxDocument.Create(messageId, consumerName, topic, payload, headersJson);
        await _context.InboxMessages.InsertOneAsync(created, cancellationToken: ct);

        _currentAttemptId = created.Id;
        return InboxAttemptDecision.Proceed;
    }

    public async Task CompleteAttemptAsync(CancellationToken ct = default)
    {
        if (_currentAttemptId is null)
            return;

        var update = Builders<InboxDocument>.Update
            .Set(m => m.Status, InboxMessageStatus.Processed)
            .Set(m => m.ProcessedAt, DateTime.UtcNow)
            .Set(m => m.NextRetryAt, (DateTime?)null)
            .Set(m => m.LastError, (string?)null);

        await _context.InboxMessages.UpdateOneAsync(m => m.Id == _currentAttemptId, update, cancellationToken: ct);
        _currentAttemptId = null;
    }

    public async Task<InboxFailureOutcome> FailAttemptAsync(
        string error,
        InboxRetryPolicy policy,
        CancellationToken ct = default)
    {
        if (_currentAttemptId is null)
            return InboxFailureOutcome.WillRetry;

        var doc = await _context.InboxMessages.Find(m => m.Id == _currentAttemptId).FirstOrDefaultAsync(ct);
        if (doc is null)
        {
            _currentAttemptId = null;
            return InboxFailureOutcome.WillRetry;
        }

        doc.MarkFailed(error, policy);
        await _context.InboxMessages.ReplaceOneAsync(m => m.Id == doc.Id, doc, cancellationToken: ct);

        var outcome = doc.Status == InboxMessageStatus.DeadLetter
            ? InboxFailureOutcome.DeadLettered
            : InboxFailureOutcome.WillRetry;

        _currentAttemptId = null;
        return outcome;
    }

    public async Task<IReadOnlyList<InboxMessageSnapshot>> GetDueForRetryAsync(int batchSize, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var docs = await _context.InboxMessages
            .Find(m => m.Status == InboxMessageStatus.Retrying && m.NextRetryAt <= now)
            .SortBy(m => m.NextRetryAt)
            .Limit(batchSize)
            .ToListAsync(ct);

        return [.. docs.Select(ToSnapshot)];
    }

    public async Task<int> DeleteProcessedBeforeAsync(DateTime olderThanUtc, int batchSize, CancellationToken ct = default)
    {
        var ids = await _context.InboxMessages
            .Find(m => m.Status == InboxMessageStatus.Processed && m.ProcessedAt < olderThanUtc)
            .SortBy(m => m.ProcessedAt)
            .Limit(batchSize)
            .Project(m => m.Id)
            .ToListAsync(ct);

        if (ids.Count == 0)
            return 0;

        var result = await _context.InboxMessages.DeleteManyAsync(
            Builders<InboxDocument>.Filter.In(m => m.Id, ids), ct);

        return (int)result.DeletedCount;
    }

    private static InboxMessageSnapshot ToSnapshot(InboxDocument m) => new(
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
