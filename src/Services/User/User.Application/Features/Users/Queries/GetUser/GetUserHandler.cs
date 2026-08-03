using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Application.Exceptions;

using Mapster;

using SmartEcommerce.User.Application.Abstractions.Persistence.UserProfiles;
using SmartEcommerce.User.Application.Abstractions.Services;

namespace SmartEcommerce.User.Application.Features.Users.Queries.GetUser;

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
