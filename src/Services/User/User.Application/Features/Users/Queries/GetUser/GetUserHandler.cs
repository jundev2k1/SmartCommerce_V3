using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;

using Mapster;

using User.Application.Abstractions.Persistence.UserProfiles;
using User.Application.Abstractions.Services;

namespace User.Application.Features.Users.Queries.GetUser;

public sealed class GetUserHandler(
    IUserProfileReadService userReadService,
    IUserDisplayNameFormatter displayNameFormatter,
    ICurrentLocaleService currentLocale) : IQueryHandler<GetUserQuery, GetUserResponse>
{
    public async Task<GetUserResponse> Handle(GetUserQuery request, CancellationToken ct = default)
    {
        var user = await userReadService.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        var displayName = displayNameFormatter.Format(user.FirstName, user.MiddleName, user.LastName, currentLocale.GetLocale());

        return user.Adapt<GetUserResponse>() with { DisplayName = displayName };
    }
}
