using BuildingBlock.Application.Abstractions.Events;

using User.Application.Abstractions.Persistence.UserProfiles;
using User.Application.Features.Users.Events.OnUserSearchRemovalRequired;

namespace User.Application.Features.Users.Events.OnUserDeletion;

public sealed class OnUserDeletionHandler(
    IUserProfileWriteService userWriteService,
    IInternalEventDispatcher eventDispatcher) : IInternalEventHandler<OnUserDeletionEvent>
{
    public async Task Handle(OnUserDeletionEvent @event, CancellationToken ct = default)
    {
        await userWriteService.DeleteWithNoTrackingAsync(@event.Id, ct);

        // Search removal trigger - dispatched inline rather than via Outbox/Kafka
        // self-consumption, since this handler already runs off an inbound Kafka message with
        // Inbox-dedup guarantees; no second hop needed. See
        // docs/tasks/2026-07-28/Task8_projection-builder-and-sync-events.md.
        await eventDispatcher.PublishAsync(new OnUserSearchRemovalRequiredEvent(@event.Id), ct);
    }
}