using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;
using SmartEcommerce.BuildingBlock.SharedKernel.Extensions;

using SmartEcommerce.Product.Application.Features.Products.Queries.GetProduct;

namespace SmartEcommerce.Product.API.Endpoints.Product;

public sealed class GetProductEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Product Details",
        "",
        "Retrieves product information by product ID. Each variation includes a live AvailableStock",
        "from Inventory Service (null if Inventory is unreachable - fail-open, not treated as 0).",
        "",
        "### Route Parameters",
        "- **productId**: Unique identifier of the product (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Product not found",
        "- **400**: Invalid productId format",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{productId}", Handle)
            .WithTags("Product")
            .RequireAuthorization(AuthorizationPoliciesConstant.RequireAuthenticated)
            .WithName("GetProduct")
            .WithDisplayName("Get Product API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetProductResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetProductQuery(productId);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetProductResponse>.Ok(response));
    }
}
