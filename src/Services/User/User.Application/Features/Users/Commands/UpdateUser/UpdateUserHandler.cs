using User.Application.Abstractions.Persistence.UserProfiles;

namespace User.Application.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserHandler(
    IUserProfileWriteService userWriteService,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    public async Task<UpdateUserResponse> Handle(UpdateUserCommand request, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await userWriteService.UpdateProfileDetailsAsync(
                request.UserId, request.FirstName.Trim(), request.LastName.Trim(), request.PhoneNumber.Trim(), ct);
        }, ct: ct);

        return new UpdateUserResponse();
    }
}
