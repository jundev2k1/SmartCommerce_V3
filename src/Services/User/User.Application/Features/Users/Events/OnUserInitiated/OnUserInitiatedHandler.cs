using SmartEcommerce.BuildingBlock.Application.Abstractions.Events;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;

using SmartEcommerce.User.Application.Abstractions.Persistence.UserProfiles;
using SmartEcommerce.User.Application.Features.Users.Events.OnUserSearchSyncRequired;

namespace SmartEcommerce.User.Application.Features.Users.Events.OnUserInitiated;

public sealed class OnUserInitiatedHandler(
    IUserProfileWriteService userWriteService,
    IInternalEventDispatcher eventDispatcher) : IInternalEventHandler<OnUserInitiatedEvent>
{
    public async Task Handle(OnUserInitiatedEvent @event, CancellationToken ct = default)
    {
        // Create user profile. This flow is only reached via Auth's self-registration path
        // (RegisterHandler), which always assigns exactly AppRole.User - never Admin.
        var user = await userWriteService.SyncFromAccountInitiationAsync(
            new SyncUserProfileRequest(
                @event.AccountId,
                @event.Email,
                @event.UserName,
                @event.PhoneNumber,
                @event.FirstName,
                @event.MiddleName,
                @event.LastName,
                [AppRoleConstant.User]),
            ct);

        // Search sync trigger - dispatched inline rather than via Outbox/Kafka self-consumption,
        // since this handler already runs in-process off Auth's gRPC call; no cross-service hop
        // to decouple from. See docs/tasks/2026-07-28/Task8_projection-builder-and-sync-events.md.
        await eventDispatcher.PublishAsync(new OnUserSearchSyncRequiredEvent(user.Id, @event.CorrelationId), ct);
    }
}
