using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Contract.Events.User;

using User.Application.Abstractions.Persistence.UserProfiles;
using User.Application.Abstractions.Services;

namespace User.Application.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserHandler(
    IUserProfileWriteService userWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser,
    IUserProfileCacheService userProfileCache) : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    public async Task<UpdateUserResponse> Handle(UpdateUserCommand request, CancellationToken ct = default)
    {
        var correlationId = currentUser.GetCorrelationId();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await userWriteService.UpdateProfileDetailsAsync(
                request.UserId,
                request.FirstName,
                request.MiddleName,
                request.LastName,
                request.PhoneNumber,
                ct);

            var integrationEvent = new UserProfileUpdatedIntegrationEvent(
                request.UserId,
                correlationId);
            await outboxStore.EnqueueAsync(integrationEvent, ct);
        }, ct: ct);

        await userProfileCache.RemoveAsync(request.UserId, ct);

        return new UpdateUserResponse();
    }
}
