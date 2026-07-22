using BuildingBlock.Application.Abstractions.Common;

using Mapster;

using User.Application.Abstractions.Repositories;

namespace User.Application.Features.Users.Queries.SearchUsers;

public sealed class SearchUsersHandler(IUserRepository userRepo)
    : IQueryHandler<SearchUsersQuery, PaginatedResult<SearchUsersItemResponse>>
{
    public async Task<PaginatedResult<SearchUsersItemResponse>> Handle(SearchUsersQuery request, CancellationToken ct = default)
    {
        var result = await userRepo.SearchAsync(request.Criteria, ct);

        var items = result.Items.Select(u => u.Adapt<SearchUsersItemResponse>()).ToList();

        return PaginatedResult<SearchUsersItemResponse>.Create(items, result.PageNumber, result.PageSize, result.TotalCount);
    }
}
