using SmartEcommerce.BuildingBlock.Application.Abstractions.Outbox;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Contract.Events.User;

using SmartEcommerce.User.Application.Abstractions.Persistence.UserProfiles;
using SmartEcommerce.User.Application.Abstractions.Services;

namespace SmartEcommerce.User.Application.Features.Users.Commands.UpdateUser;

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
