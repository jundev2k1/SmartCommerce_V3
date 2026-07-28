using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Application.Abstractions.Services;

using Mapster;

using User.Application.Abstractions.Persistence.UserProfiles;
using User.Application.Abstractions.Services;

namespace User.Application.Features.Users.Queries.SearchUsers;

public sealed class SearchUsersHandler(
    IUserProfileReadService userReadService,
    IUserDisplayNameFormatter displayNameFormatter,
    ICurrentLocaleService currentLocale) : IQueryHandler<SearchUsersQuery, PaginatedResult<SearchUsersItemResponse>>
{
    public async Task<PaginatedResult<SearchUsersItemResponse>> Handle(SearchUsersQuery request, CancellationToken ct = default)
    {
        var result = await userReadService.SearchAsync(request.Criteria, ct);
        var locale = currentLocale.GetLocale();

        var items = result.Items
            .Select(u => u.Adapt<SearchUsersItemResponse>() with
            {
                DisplayName = displayNameFormatter.Format(u.FirstName, u.MiddleName, u.LastName, locale)
            })
            .ToList();

        return PaginatedResult<SearchUsersItemResponse>.Create(items, result.PageNumber, result.PageSize, result.TotalCount);
    }
}
