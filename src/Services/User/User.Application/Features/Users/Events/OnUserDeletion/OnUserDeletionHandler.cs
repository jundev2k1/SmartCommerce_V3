using BuildingBlock.Application.Abstractions.Events;

using User.Application.Abstractions.Persistence.UserProfiles;

namespace User.Application.Features.Users.Events.OnUserDeletion;

public sealed class OnUserDeletionHandler(
    IUserProfileWriteService userWriteService) : IInternalEventHandler<OnUserDeletionEvent>
{
    public async Task Handle(OnUserDeletionEvent @event, CancellationToken ct = default)
    {
        await userWriteService.DeleteWithNoTrackingAsync(@event.Id, ct);
    }
}