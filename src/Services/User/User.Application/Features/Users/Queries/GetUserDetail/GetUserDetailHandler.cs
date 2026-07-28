using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;

using User.Application.Abstractions.Persistence.UserProfiles;
using User.Application.Abstractions.Services;

namespace User.Application.Features.Users.Queries.GetUserDetail;

public sealed class GetUserDetailHandler(
    ICurrentUserService currentUser,
    ICurrentLocaleService currentLocale,
    IUserProfileReadService userReadService,
    IRoleCacheReader roleCacheReader,
    IUserDisplayNameFormatter displayNameFormatter) : IQueryHandler<GetUserDetailQuery, GetUserDetailResponse>
{
    public async Task<GetUserDetailResponse> Handle(GetUserDetailQuery request, CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId()
            ?? throw new UnauthorizedException();

        var user = await userReadService.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("UserProfile", userId);

        var roles = await roleCacheReader.GetUserRolesAsync(userId, ct);
        var displayName = displayNameFormatter.Format(user.FirstName, user.MiddleName, user.LastName, currentLocale.GetLocale());

        return new GetUserDetailResponse(
            user.Id,
            user.Email,
            user.UserName,
            user.PhoneNumber,
            user.FirstName,
            user.MiddleName,
            user.LastName,
            displayName,
            user.Status,
            roles,
            user.CreatedAt,
            user.UpdatedAt);
    }
}
