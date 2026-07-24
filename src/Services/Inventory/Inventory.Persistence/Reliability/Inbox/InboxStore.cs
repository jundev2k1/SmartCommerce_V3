using BuildingBlock.Application.Abstractions.Outbox;

namespace Inventory.Persistence.Inbox;

/// <summary>
/// Application-level adapter: delegates to the generic EF store, translating between the two
/// layers' independently-defined Inbox DTOs/enums (same convention as Outbox's dual snapshot).
/// </summary>
public sealed class InboxStore(BuildingBlock.Persistence.Inbox.IInboxStore primitiveStore) : BuildingBlock.Application.Abstractions.Outbox.IInboxStore
{
    private readonly BuildingBlock.Persistence.Inbox.IInboxStore _primitiveStore = primitiveStore;

    public async Task<InboxAttemptDecision> BeginAttemptAsync(
        Guid messageId,
        string consumerName,
        string topic,
        string payload,
        string headersJson,
        CancellationToken ct = default)
    {
        var decision = await _primitiveStore.BeginAttemptAsync(messageId, consumerName, topic, payload, headersJson, ct);
        return ToApplication(decision);
    }

    public Task CompleteAttemptAsync(CancellationToken ct = default) =>
        _primitiveStore.CompleteAttemptAsync(ct);

    public async Task<InboxFailureOutcome> FailAttemptAsync(string error, InboxRetryPolicy policy, CancellationToken ct = default)
    {
        var primitivePolicy = new BuildingBlock.Persistence.Inbox.InboxRetryPolicy(
            policy.MaxRetryCount, policy.InitialRetryDelay, policy.RetryBackoffMultiplier, policy.MaximumRetryDelay);

        var outcome = await _primitiveStore.FailAttemptAsync(error, primitivePolicy, ct);
        return ToApplication(outcome);
    }

    public async Task<IReadOnlyList<InboxMessageSnapshot>> GetDueForRetryAsync(int batchSize, CancellationToken ct = default)
    {
        var rows = await _primitiveStore.GetDueForRetryAsync(batchSize, ct);
        return [.. rows.Select(ToApplication)];
    }

    public Task<int> DeleteProcessedBeforeAsync(DateTime olderThanUtc, int batchSize, CancellationToken ct = default) =>
        _primitiveStore.DeleteProcessedBeforeAsync(olderThanUtc, batchSize, ct);

    private static InboxAttemptDecision ToApplication(BuildingBlock.Persistence.Inbox.InboxAttemptDecision decision) => decision switch
    {
        BuildingBlock.Persistence.Inbox.InboxAttemptDecision.Proceed => InboxAttemptDecision.Proceed,
        BuildingBlock.Persistence.Inbox.InboxAttemptDecision.AlreadyProcessed => InboxAttemptDecision.AlreadyProcessed,
        BuildingBlock.Persistence.Inbox.InboxAttemptDecision.DeadLettered => InboxAttemptDecision.DeadLettered,
        BuildingBlock.Persistence.Inbox.InboxAttemptDecision.NotDueYet => InboxAttemptDecision.NotDueYet,
        _ => throw new ArgumentOutOfRangeException(nameof(decision), decision, null),
    };

    private static InboxFailureOutcome ToApplication(BuildingBlock.Persistence.Inbox.InboxFailureOutcome outcome) => outcome switch
    {
        BuildingBlock.Persistence.Inbox.InboxFailureOutcome.AlreadyCommitted => InboxFailureOutcome.AlreadyCommitted,
        BuildingBlock.Persistence.Inbox.InboxFailureOutcome.WillRetry => InboxFailureOutcome.WillRetry,
        BuildingBlock.Persistence.Inbox.InboxFailureOutcome.DeadLettered => InboxFailureOutcome.DeadLettered,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null),
    };

    private static InboxMessageStatus ToApplication(BuildingBlock.Persistence.Inbox.InboxMessageStatus status) => status switch
    {
        BuildingBlock.Persistence.Inbox.InboxMessageStatus.Pending => InboxMessageStatus.Pending,
        BuildingBlock.Persistence.Inbox.InboxMessageStatus.Retrying => InboxMessageStatus.Retrying,
        BuildingBlock.Persistence.Inbox.InboxMessageStatus.Processed => InboxMessageStatus.Processed,
        BuildingBlock.Persistence.Inbox.InboxMessageStatus.DeadLetter => InboxMessageStatus.DeadLetter,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };

    private static InboxMessageSnapshot ToApplication(BuildingBlock.Persistence.Inbox.InboxMessageSnapshot snapshot) => new(
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
}
