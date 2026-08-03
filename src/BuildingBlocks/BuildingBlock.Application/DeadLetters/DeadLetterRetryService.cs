using System.Diagnostics;
using System.Text.Json;

using SmartEcommerce.BuildingBlock.Application.Abstractions.Idempotency;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Outbox;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Messaging.Abstractions;

using Microsoft.Extensions.Logging;

namespace SmartEcommerce.BuildingBlock.Application.DeadLetters;

public sealed class DeadLetterRetryService(
    IInboxStore inboxStore,
    IOutboxPublisher outboxPublisher,
    IDistributedLockProvider? lockProvider,
    ICurrentUserService currentUser,
    ILogger<DeadLetterRetryService> logger) : IDeadLetterRetryService
{
    private static readonly TimeSpan LockExpiration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(2);

    public async Task<DeadLetterRetryAttemptResult> RetryAsync(Guid inboxMessageId, CancellationToken ct = default)
    {
        var operatorId = currentUser.GetUserId()?.ToString();
        logger.LogInformation(
            "Dead-letter retry requested for InboxMessage {InboxMessageId} by {Operator}",
            inboxMessageId, operatorId ?? "unknown");

        // Defense-in-depth against thundering-herd bulk/retry-all calls hitting the same row -
        // RequeueDeadLetterAsync's conditional update already guarantees correctness on its own,
        // so this lock is skipped (not required) on services with no Redis/IDistributedLockProvider
        // registered rather than forcing a new infrastructure dependency onto them.
        IDistributedLock? @lock = null;
        if (lockProvider is not null)
        {
            var lockKey = $"deadletter-retry:{inboxMessageId}";
            @lock = await lockProvider.AcquireAsync(lockKey, LockExpiration, LockTimeout, ct);
            if (@lock is null)
            {
                logger.LogWarning("Dead-letter retry conflict for InboxMessage {InboxMessageId}: already retrying", inboxMessageId);
                return new DeadLetterRetryAttemptResult(inboxMessageId, DeadLetterRetryOutcome.Conflict, null);
            }
        }

        await using var _ = @lock;

        var requeue = await inboxStore.RequeueDeadLetterAsync(inboxMessageId, operatorId, ct);
        if (requeue.Outcome == InboxRequeueOutcome.NotFound)
            return new DeadLetterRetryAttemptResult(inboxMessageId, DeadLetterRetryOutcome.NotFound, null);
        if (requeue.Outcome == InboxRequeueOutcome.NotDeadLetter)
            return new DeadLetterRetryAttemptResult(inboxMessageId, DeadLetterRetryOutcome.NotDeadLetter, null);

        var snapshot = requeue.Snapshot!;
        logger.LogInformation(
            "Dead-letter retry started for InboxMessage {InboxMessageId} ({ConsumerName}/{Topic}), attempt #{RetryNumber}",
            inboxMessageId, snapshot.ConsumerName, snapshot.Topic, requeue.RetryNumber);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var (eventType, correlationId, actorId, actorType) = ParseHeaders(snapshot.HeadersJson);
            await outboxPublisher.PublishOutboxMessageAsync(
                snapshot.MessageId, snapshot.Topic, snapshot.Payload, eventType, correlationId, actorId, actorType, ct);

            logger.LogInformation(
                "Dead-letter retry republished InboxMessage {InboxMessageId} to {Topic} in {DurationMs}ms - " +
                "outcome (Succeeded/FailedAgain) will be recorded when the redelivered message is next processed",
                inboxMessageId, snapshot.Topic, stopwatch.ElapsedMilliseconds);

            return new DeadLetterRetryAttemptResult(inboxMessageId, DeadLetterRetryOutcome.Succeeded, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Dead-letter retry failed to republish InboxMessage {InboxMessageId} after {DurationMs}ms - reverting to DeadLetter",
                inboxMessageId, stopwatch.ElapsedMilliseconds);

            await inboxStore.RevertFailedRequeueAsync(inboxMessageId, ex.Message, ct);
            return new DeadLetterRetryAttemptResult(inboxMessageId, DeadLetterRetryOutcome.PublishFailed, ex.Message);
        }
    }

    /// <summary>
    /// Recovers the fields KafkaOutboxPublisher originally wrote as headers (see
    /// SmartEcommerce.BuildingBlock.Messaging.Kafka.Publishers.KafkaOutboxPublisher) from the dedup-header JSON
    /// captured on first delivery, so the republish carries the same event-type/correlation/actor
    /// metadata as the original publish.
    /// </summary>
    private static (string EventType, string CorrelationId, string? ActorId, string ActorType) ParseHeaders(string headersJson)
    {
        var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersJson) ?? [];

        headers.TryGetValue("event-type", out var eventType);
        headers.TryGetValue("correlation-id", out var correlationId);
        headers.TryGetValue("actor-type", out var actorType);
        headers.TryGetValue("actor-id", out var actorId);

        return (eventType ?? string.Empty, correlationId ?? Guid.NewGuid().ToString(), actorId, actorType ?? "System");
    }
}
