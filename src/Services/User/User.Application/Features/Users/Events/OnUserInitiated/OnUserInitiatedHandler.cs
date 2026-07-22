using BuildingBlock.Application.Abstractions.Events;

using User.Application.Abstractions.Repositories;

namespace User.Application.Features.Users.Events.OnUserInitiated;

public sealed class OnUserInitiatedHandler(
    IUnitOfWork uow,
    IUserRepository userRepo) : IInternalEventHandler<OnUserInitiatedEvent>
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
        await userRepo.AddAsync(user, ct);
        await uow.SaveChangesAsync(ct);
    }
}
