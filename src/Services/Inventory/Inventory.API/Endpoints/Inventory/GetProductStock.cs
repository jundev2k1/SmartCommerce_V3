using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;
using SmartEcommerce.BuildingBlock.SharedKernel.Extensions;

using SmartEcommerce.Inventory.Application.Features.Inventories.Queries.GetProductStock;

namespace SmartEcommerce.Inventory.API.Endpoints.Inventory;

public sealed class GetProductStockEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Product Stock",
        "",
        "Returns total stock for a product. ProductId alone rolls up every variation and",
        "warehouse; adding ProductVariationId narrows the rollup to that single variation.",
        "This is the same query the gRPC stock lookup (consumed by Order Service) is backed by.",
        "",
        "### Route Parameters",
        "- **productId**: Unique identifier of the product (required, must be valid GUID)",
        "",
        "### Query Parameters",
        "- **productVariationId**: Optional, narrows the rollup to a single variation",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{productId}/stock", Handle)
            .WithTags("Inventory")
            .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated)
            .WithName("GetProductStock")
            .WithDisplayName("Get Product Stock API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetProductStockResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        [FromQuery] Guid? productVariationId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetProductStockQuery(productId, productVariationId);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetProductStockResponse>.Ok(response));
    }
}
