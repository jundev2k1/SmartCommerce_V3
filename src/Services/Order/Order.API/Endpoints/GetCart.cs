using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Extensions;

using Order.Application.Abstractions.Services;
using Order.Application.Features.Cart.Queries.GetCart;

namespace Order.API.Endpoints;

public sealed class GetCartEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Cart",
        "",
        "Returns the current user's cart. Lines are always priced/named live from the product",
        "catalog - a line whose variation was deleted or made unavailable since it was added is",
        "dropped automatically rather than being returned stale.",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/cart", Handle)
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
