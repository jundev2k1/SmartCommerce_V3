using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Extensions;

using Product.Application.Features.ProductCategories.Queries.ListProductCategories;

namespace Product.API.Endpoints;

public sealed class ListProductCategoriesEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## List Product Categories",
        "",
        "Returns the full flat category set (Id/ParentCategoryId pairs) so a client can assemble",
        "the hierarchy tree itself. Not paginated - category counts are small reference data.",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories", Handle)
            .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated)
            .WithName("ListProductCategories")
            .WithDisplayName("List Product Categories API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<ListProductCategoriesResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new ListProductCategoriesQuery(), ct);

        return Results.Ok(ApiResponse<ListProductCategoriesResponse>.Ok(response));
    }
}
