using User.Application.Abstractions.Repositories;

namespace User.Application.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserHandler(
    IUserRepository userRepo,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    public async Task<UpdateUserResponse> Handle(UpdateUserCommand request, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await userRepo.UpdateAsync(request.UserId, async (user) =>
            {
                user.UpdateProfile(
                    request.FirstName.Trim(),
                    request.LastName.Trim(),
                    request.PhoneNumber.Trim());
            }, ct);
        }, ct: ct);

        return new UpdateUserResponse();
    }
}
