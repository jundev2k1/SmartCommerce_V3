using System.Text.Json;

using SmartEcommerce.BuildingBlock.Application.Abstractions.Events;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Contract.Events.User;
using SmartEcommerce.BuildingBlock.Messaging.Abstractions;

using SmartEcommerce.User.Application.Features.Users.Events.OnUserDeletion;

namespace SmartEcommerce.User.Infrastructure.Messaging.Consumers;

public sealed class UserAccountDeletionIntegrationEventConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<UserAccountDeletionIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(UserDeletionIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        // Deliberately not caught here: the Inbox attempt executor (see
        // SmartEcommerce.BuildingBlock.Infrastructure.Messaging.InboxAttemptExecutor) needs the exception to
        // propagate so it can record the failure and schedule a retry. Swallowing it here would
        // make every attempt look like a success and the message would never be retried.
        var integrationEvent = JsonSerializer.Deserialize<UserDeletionIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize UserAccountDeletionIntegrationEvent");

        logger.Information(
            "Received UserAccountDeletionIntegrationEvent for UserId: {UserId}",
            integrationEvent.UserId);

        var @event = new OnUserDeletionEvent(
            Guid.Parse(integrationEvent.UserId));
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information(
            "Successfully processed UserAccountDeletionIntegrationEvent for UserId: {UserId}",
            integrationEvent.UserId);
    }
}
