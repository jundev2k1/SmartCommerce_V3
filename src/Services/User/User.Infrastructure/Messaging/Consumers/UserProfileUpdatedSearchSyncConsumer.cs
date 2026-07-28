using System.Text.Json;

using BuildingBlock.Application.Abstractions.Events;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Contract.Events.User;
using BuildingBlock.Messaging.Abstractions;

using User.Application.Features.Users.Events.OnUserSearchSyncRequired;

namespace User.Infrastructure.Messaging.Consumers;

/// <summary>User self-consumes its own integration events to keep the Search index in sync - see docs/reference/search.md.</summary>
public sealed class UserProfileUpdatedSearchSyncConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<UserProfileUpdatedSearchSyncConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(UserProfileUpdatedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message, IReadOnlyDictionary<string, string> headers, CancellationToken ct = default)
    {
        var integrationEvent = JsonSerializer.Deserialize<UserProfileUpdatedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize UserProfileUpdatedIntegrationEvent");

        logger.Information("Received UserProfileUpdatedIntegrationEvent for UserId: {UserId}", integrationEvent.UserId);

        var @event = new OnUserSearchSyncRequiredEvent(integrationEvent.UserId, integrationEvent.CorrelationId);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information("Successfully processed UserProfileUpdatedIntegrationEvent for UserId: {UserId}", integrationEvent.UserId);
    }
}
