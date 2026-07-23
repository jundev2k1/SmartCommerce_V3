using BuildingBlock.Application.Abstractions.Common;

using Mapster;

using User.Application.Abstractions.Persistence.UserProfiles;

namespace User.Application.Features.Users.Queries.SearchUsers;

public sealed class SearchUsersHandler(IUserProfileReadService userReadService)
    : IQueryHandler<SearchUsersQuery, PaginatedResult<SearchUsersItemResponse>>
{
    public async Task<PaginatedResult<SearchUsersItemResponse>> Handle(SearchUsersQuery request, CancellationToken ct = default)
    {
        var result = await userReadService.SearchAsync(request.Criteria, ct);

        var items = result.Items.Select(u => u.Adapt<SearchUsersItemResponse>()).ToList();

        return PaginatedResult<SearchUsersItemResponse>.Create(items, result.PageNumber, result.PageSize, result.TotalCount);
    }
}
