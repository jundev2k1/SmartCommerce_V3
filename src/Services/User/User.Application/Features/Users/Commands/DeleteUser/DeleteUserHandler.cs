using BuildingBlock.Application.Abstractions.Services;

using User.Application.Abstractions.Repositories;

namespace User.Application.Features.Users.Commands.DeleteUser;

public sealed class DeleteUserHandler(
    IUserRepository userRepo,
    IUnitOfWork unitOfWork,
    IAppLogger<DeleteUserHandler> logger)
    : ICommandHandler<DeleteUserCommand, DeleteUserProfileResponse>
{
    public async Task<DeleteUserProfileResponse> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        var existing = await userRepo.GetByIdAsync(request.UserId, ct);
        if (existing is null)
        {
            logger.Information(
                "UserProfile {UserId} already absent, skipping compensating deletion",
                request.UserId);
            return new DeleteUserProfileResponse(Deleted: false);
        }

        await userRepo.DeleteAsync(request.UserId, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.Warning(
            "Deleted UserProfile {UserId} due to compensating rollback. Reason: {Reason}. CorrelationId: {CorrelationId}",
            request.UserId,
            request.Reason,
            request.CorrelationId);

        return new DeleteUserProfileResponse(Deleted: true);
    }
}
