using User.Application.Abstractions.Persistence.UserProfiles;

namespace User.Application.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserHandler(
    IUserProfileWriteService userWriteService) : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    public async Task<UpdateUserResponse> Handle(UpdateUserCommand request, CancellationToken ct = default)
    {
        await userWriteService.UpdateProfileAsync(request.UserId, async (user) =>
        {
            user.UpdateProfile(
                request.FirstName.Trim(),
                request.LastName.Trim(),
                request.PhoneNumber.Trim());
        }, ct);

        return new UpdateUserResponse();
    }
}
