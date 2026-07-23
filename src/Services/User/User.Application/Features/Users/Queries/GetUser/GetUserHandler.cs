using BuildingBlock.Application.Exceptions;

using Mapster;

using User.Application.Abstractions.Persistence.UserProfiles;

namespace User.Application.Features.Users.Queries.GetUser;

public sealed class GetUserHandler(IUserProfileReadService userReadService)
    : IQueryHandler<GetUserQuery, GetUserResponse>
{
    public async Task<GetUserResponse> Handle(GetUserQuery request, CancellationToken ct = default)
    {
        var user = await userReadService.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        return user.Adapt<GetUserResponse>();
    }
}
