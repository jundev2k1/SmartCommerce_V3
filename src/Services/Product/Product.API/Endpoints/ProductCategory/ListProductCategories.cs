using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;
using SmartEcommerce.BuildingBlock.SharedKernel.Extensions;

using SmartEcommerce.Product.Application.Features.ProductCategories.Queries.ListProductCategories;

namespace SmartEcommerce.Product.API.Endpoints.ProductCategory;

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
            .WithTags("ProductCategory")
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
