using BuildingBlock.Application.Abstractions.Events;

using User.Application.Abstractions.Repositories;

namespace User.Application.Features.Users.Events.OnUserDeletion;

public sealed class OnUserDeletionHandler(
    IUserRepository userRepo) : IInternalEventHandler<OnUserDeletionEvent>
{
    public async Task Handle(OnUserDeletionEvent @event, CancellationToken ct = default)
    {
        await userRepo.DeleteWithNoTrackingAsync(@event.Id, ct);
    }
}