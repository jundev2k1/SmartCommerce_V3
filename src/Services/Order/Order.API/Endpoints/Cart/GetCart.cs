using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;
using SmartEcommerce.BuildingBlock.SharedKernel.Extensions;

using SmartEcommerce.Order.Application.Abstractions.Services;
using SmartEcommerce.Order.Application.Features.Cart.Queries.GetCart;

namespace SmartEcommerce.Order.API.Endpoints.Cart;

public sealed class GetCartEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Cart",
        "",
        "Returns the current user's cart. Lines are always priced/named live from the product",
        "catalog - a line whose variation was deleted or made unavailable since it was added is",
        "dropped automatically rather than being returned stale. Each remaining line also carries",
        "a live AvailableStock and IsInsufficientStock (true when Quantity exceeds AvailableStock) -",
        "an insufficient-stock line is kept and marked, never silently dropped.",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/cart", Handle)
            .WithTags("Cart")
            .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated)
            .WithName("GetCart")
            .WithDisplayName("Get Cart API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CartResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetCartQuery(), ct);

        return Results.Ok(ApiResponse<CartResponse>.Ok(response));
    }
}
