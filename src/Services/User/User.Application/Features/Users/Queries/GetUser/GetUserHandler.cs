using BuildingBlock.Application.Exceptions;

using Mapster;

using User.Application.Abstractions.Repositories;

namespace User.Application.Features.Users.Queries.GetUser;

public sealed class GetUserHandler(IUserRepository userRepo)
    : IQueryHandler<GetUserQuery, GetUserResponse>
{
    public async Task<GetUserResponse> Handle(GetUserQuery request, CancellationToken ct = default)
    {
        var user = await userRepo.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        return user.Adapt<GetUserResponse>();
    }
}
