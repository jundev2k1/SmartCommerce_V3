using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Extensions;

using User.Application.Features.Users.Queries.SearchUsers;

namespace User.API.Endpoints;

public sealed class SearchUsersEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Search Users",
        "",
        "Admin/Root only. Paginated, filterable, sortable search over user profiles, backed by Postgres indexes.",
        "",
        "### Request Body",
        "- **keyword**: Free-text match against username/email (case-insensitive)/first name/last name (optional)",
        "- **filters**: `[{ field, operator, value }]` - e.g. `{ \"field\": \"phone\", \"operator\": \"sw\", \"value\": \"0901\" }` for prefix search, `\"operator\": \"ew\"` for suffix search (both index-backed, never a full scan)",
        "- **sorts**: `[{ field, direction }]` - direction is `asc`/`desc`",
        "- **page** / **pageSize**: 1-based paging (default 1 / 20)",
        "",
        "### Searchable Fields",
        "- **userName**, **email**: eq/ne/c/sw/ew/in/nin, sortable, included in keyword search, case-insensitive",
        "- **firstName**, **lastName**: eq/ne/c/sw/ew/in/nin, included in keyword search",
        "- **status**: eq/ne/in/nin, sortable",
        "- **phone**: eq/ne/sw/ew/in/nin (indexed prefix/suffix search - `c` is intentionally not supported)",
        "- **createdAt**: eq/ne/gt/gte/lt/lte/between, sortable",
        "- **role**: eq/ne against each row's role snapshot (GIN-indexed array-containment, not sortable)",
        "",
        "### Error Responses",
        "- **400**: Unknown field, operator not allowed for the field, or malformed value",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/search", Handle)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("SearchUsers")
            .WithDisplayName("Search Users API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<PaginatedResult<SearchUsersItemResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromBody] CriteriaRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new SearchUsersQuery(request);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<PaginatedResult<SearchUsersItemResponse>>.Ok(response));
    }
}
