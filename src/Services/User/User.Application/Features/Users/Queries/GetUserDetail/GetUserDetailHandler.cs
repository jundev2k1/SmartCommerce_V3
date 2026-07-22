using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;

using User.Application.Abstractions.Repositories;
using User.Application.Abstractions.Services;

namespace User.Application.Features.Users.Queries.GetUserDetail;

public sealed class GetUserDetailHandler(
    ICurrentUserService currentUser,
    IUserRepository userRepo,
    IRoleCacheReader roleCacheReader) : IQueryHandler<GetUserDetailQuery, GetUserDetailResponse>
{
    public async Task<GetUserDetailResponse> Handle(GetUserDetailQuery request, CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId()
            ?? throw new UnauthorizedException();

        var user = await userRepo.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("UserProfile", userId);

        var roles = await roleCacheReader.GetUserRolesAsync(userId, ct);

        return new GetUserDetailResponse(
            user.Id,
            user.Email,
            user.UserName,
            user.PhoneNumber,
            user.FirstName,
            user.LastName,
            user.Status,
            roles,
            user.CreatedAt,
            user.UpdatedAt);
    }
}
