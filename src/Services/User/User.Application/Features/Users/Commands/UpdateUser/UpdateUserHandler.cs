using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Contract.Events.User;

using User.Application.Abstractions.Persistence.UserProfiles;

namespace User.Application.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserHandler(
    IUserProfileWriteService userWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    public async Task<UpdateUserResponse> Handle(UpdateUserCommand request, CancellationToken ct = default)
    {
        var correlationId = currentUser.GetCorrelationId() ?? Guid.NewGuid().ToString();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await userWriteService.UpdateProfileDetailsAsync(
                request.UserId, request.FirstName.Trim(), request.MiddleName.Trim(), request.LastName.Trim(), request.PhoneNumber.Trim(), ct);

            // Search sync trigger - see docs/reference/search.md and
            // docs/tasks/2026-07-28/Task8_projection-builder-and-sync-events.md. Without this,
            // the search index would never learn about a profile edit.
            var integrationEvent = new UserProfileUpdatedIntegrationEvent(request.UserId, correlationId);
            await outboxStore.EnqueueAsync(integrationEvent, ct);
        }, ct: ct);

        return new UpdateUserResponse();
    }
}
