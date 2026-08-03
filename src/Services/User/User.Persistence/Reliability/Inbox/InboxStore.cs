using SmartEcommerce.BuildingBlock.Application.Abstractions.Outbox;

namespace SmartEcommerce.User.Persistence.Reliability.Inbox;

/// <summary>
/// Application-level adapter: delegates to the generic EF store, translating between the two
/// layers' independently-defined Inbox DTOs/enums (same convention as Outbox's dual snapshot).
/// </summary>
public sealed class InboxStore(SmartEcommerce.BuildingBlock.Persistence.Inbox.IInboxStore primitiveStore) : SmartEcommerce.BuildingBlock.Application.Abstractions.Outbox.IInboxStore
{
    private readonly SmartEcommerce.BuildingBlock.Persistence.Inbox.IInboxStore _primitiveStore = primitiveStore;

    public async Task<InboxAttemptDecision> BeginAttemptAsync(
        Guid messageId,
        string consumerName,
        string topic,
        string payload,
        string headersJson,
        CancellationToken ct = default)
    {
        var decision = await _primitiveStore.BeginAttemptAsync(
            messageId,
            consumerName,
            topic,
            payload,
            headersJson,
            ct);
        return ToApplication(decision);
    }

    public Task CompleteAttemptAsync(Guid messageId, string consumerName, CancellationToken ct = default) =>
        _primitiveStore.CompleteAttemptAsync(messageId, consumerName, ct);

    public async Task<InboxFailureOutcome> FailAttemptAsync(
        Guid messageId,
        string consumerName,
        string error,
        InboxRetryPolicy policy,
        CancellationToken ct = default)
    {
        var primitivePolicy = new SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxRetryPolicy(
            policy.MaxRetryCount,
            policy.InitialRetryDelay,
            policy.RetryBackoffMultiplier,
            policy.MaximumRetryDelay);

        var outcome = await _primitiveStore.FailAttemptAsync(
            messageId,
            consumerName,
            error,
            primitivePolicy,
            ct);
        return ToApplication(outcome);
    }

    public async Task<IReadOnlyList<InboxMessageSnapshot>> GetDueForRetryAsync(int batchSize, CancellationToken ct = default)
    {
        var rows = await _primitiveStore.GetDueForRetryAsync(batchSize, ct);
        return [.. rows.Select(ToApplication)];
    }

    public Task<int> DeleteProcessedBeforeAsync(DateTime olderThanUtc, int batchSize, CancellationToken ct = default) =>
        _primitiveStore.DeleteProcessedBeforeAsync(olderThanUtc, batchSize, ct);

    public async Task<IReadOnlyList<InboxDeadLetterSummary>> GetDeadLetterSummaryAsync(CancellationToken ct = default)
    {
        var rows = await _primitiveStore.GetDeadLetterSummaryAsync(ct);
        return [.. rows.Select(ToApplication)];
    }

    public async Task<InboxRequeueResult> RequeueDeadLetterAsync(Guid inboxMessageId, string? operatorId, CancellationToken ct = default)
    {
        var result = await _primitiveStore.RequeueDeadLetterAsync(inboxMessageId, operatorId, ct);
        return ToApplication(result);
    }

    public async Task<IReadOnlyList<InboxRetryHistoryEntry>> GetRetryHistoryAsync(Guid inboxMessageId, CancellationToken ct = default)
    {
        var rows = await _primitiveStore.GetRetryHistoryAsync(inboxMessageId, ct);
        return [.. rows.Select(ToApplication)];
    }

    public Task RevertFailedRequeueAsync(Guid inboxMessageId, string error, CancellationToken ct = default) =>
        _primitiveStore.RevertFailedRequeueAsync(inboxMessageId, error, ct);

    private static InboxAttemptDecision ToApplication(SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxAttemptDecision decision) => decision switch
    {
        SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxAttemptDecision.Proceed => InboxAttemptDecision.Proceed,
        SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxAttemptDecision.AlreadyProcessed => InboxAttemptDecision.AlreadyProcessed,
        SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxAttemptDecision.DeadLettered => InboxAttemptDecision.DeadLettered,
        SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxAttemptDecision.NotDueYet => InboxAttemptDecision.NotDueYet,
        _ => throw new ArgumentOutOfRangeException(nameof(decision), decision, null),
    };

    private static InboxFailureOutcome ToApplication(SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxFailureOutcome outcome) => outcome switch
    {
        SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxFailureOutcome.AlreadyCommitted => InboxFailureOutcome.AlreadyCommitted,
        SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxFailureOutcome.WillRetry => InboxFailureOutcome.WillRetry,
        SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxFailureOutcome.DeadLettered => InboxFailureOutcome.DeadLettered,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null),
    };

    private static InboxMessageStatus ToApplication(SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxMessageStatus status) => status switch
    {
        SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxMessageStatus.Pending => InboxMessageStatus.Pending,
        SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxMessageStatus.Retrying => InboxMessageStatus.Retrying,
        SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxMessageStatus.Processed => InboxMessageStatus.Processed,
        SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxMessageStatus.DeadLetter => InboxMessageStatus.DeadLetter,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };

    private static InboxMessageSnapshot ToApplication(SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxMessageSnapshot snapshot) => new(
        snapshot.MessageId,
        snapshot.ConsumerName,
        snapshot.Topic,
        snapshot.Payload,
        snapshot.HeadersJson,
        ToApplication(snapshot.Status),
        snapshot.RetryCount,
        snapshot.CreatedAt,
        snapshot.ProcessedAt,
        snapshot.NextRetryAt,
        snapshot.LastRetryAt,
        snapshot.LastError);

    private static InboxDeadLetterSummary ToApplication(SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxDeadLetterSummary summary) => new(
        summary.ConsumerName, summary.Topic, summary.Count, summary.OldestDeadLetteredAt);

    private static InboxRequeueResult ToApplication(SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxRequeueResult result) => new(
        result.Outcome switch
        {
            SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxRequeueOutcome.Requeued => InboxRequeueOutcome.Requeued,
            SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxRequeueOutcome.NotFound => InboxRequeueOutcome.NotFound,
            SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxRequeueOutcome.NotDeadLetter => InboxRequeueOutcome.NotDeadLetter,
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Outcome, null),
        },
        result.Snapshot is null ? null : ToApplication(result.Snapshot),
        result.RetryNumber);

    private static InboxRetryHistoryEntry ToApplication(SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxRetryHistoryEntry entry) => new(
        entry.Id, entry.InboxMessageId, entry.MessageId, entry.ConsumerName, entry.Topic, entry.RetryNumber,
        entry.StartedAt, entry.FinishedAt, entry.DurationMs, entry.Operator,
        entry.Result switch
        {
            SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxRetryHistoryResult.Retrying => InboxRetryHistoryResult.Retrying,
            SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxRetryHistoryResult.Succeeded => InboxRetryHistoryResult.Succeeded,
            SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxRetryHistoryResult.FailedAgain => InboxRetryHistoryResult.FailedAgain,
            SmartEcommerce.BuildingBlock.Persistence.Inbox.InboxRetryHistoryResult.Cancelled => InboxRetryHistoryResult.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(entry), entry.Result, null),
        },
        entry.Exception);
}
