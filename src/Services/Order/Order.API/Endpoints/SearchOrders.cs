using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Extensions;

using Order.Application.Features.Orders.Queries.SearchOrders;

namespace Order.API.Endpoints;

public sealed class SearchOrdersEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Search Orders",
        "",
        "Admin search over orders. Supports filtering/sorting by creation date, customer name,",
        "phone number (prefix or suffix match), and status, plus free-text keyword search over",
        "customer name.",
        "",
        "### Request Body",
        "- **Keyword**: Free-text match against keyword-searchable fields (CustomerName)",
        "- **Filters**: List of `{ field, operator, value }` - allowed fields: customerName, phone, status, createdAt",
        "- **Sorts**: List of `{ field, direction }` - sortable fields: customerName, status, createdAt",
        "- **Page** / **PageSize**: Pagination (PageSize 1-200, default 20)",
        "",
        "### Error Responses",
        "- **400**: Unknown field, disallowed operator for a field, or malformed filter/sort value",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders/search", Handle)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("SearchOrders")
            .WithDisplayName("Search Orders API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<PaginatedResult<SearchOrdersItemResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromBody] CriteriaRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new SearchOrdersQuery(request);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<PaginatedResult<SearchOrdersItemResponse>>.Ok(response));
    }
}
