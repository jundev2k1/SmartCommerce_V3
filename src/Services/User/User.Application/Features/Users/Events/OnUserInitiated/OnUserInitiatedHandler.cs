using BuildingBlock.Application.Abstractions.Events;

using User.Application.Abstractions.Persistence.UserProfiles;

namespace User.Application.Features.Users.Events.OnUserInitiated;

public sealed class OnUserInitiatedHandler(
    IUserProfileWriteService userWriteService) : IInternalEventHandler<OnUserInitiatedEvent>
{
    public async Task Handle(OnUserInitiatedEvent @event, CancellationToken ct = default)
    {
        // Create user profile
        var user = UserProfile.Create(
            @event.AccountId,
            @event.Email,
            @event.UserName,
            @event.PhoneNumber,
            @event.FirstName,
            @event.LastName);
        await userWriteService.SyncFromAccountInitiationAsync(user, ct);
    }
}
