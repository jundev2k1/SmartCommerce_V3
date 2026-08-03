using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;
using SmartEcommerce.BuildingBlock.SharedKernel.Extensions;

using SmartEcommerce.Order.Application.Abstractions.Services;
using SmartEcommerce.Order.Application.Features.Cart.Commands.RemoveCartItem;

namespace SmartEcommerce.Order.API.Endpoints.Cart;

public sealed class RemoveCartItemEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Remove Cart Item",
        "",
        "Removes a single line item from the current user's cart. A no-op (still 200) if the",
        "variation wasn't in the cart.",
        "",
        "### Route Parameters",
        "- **variationId**: Id of the product variation to remove (required)",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/cart/items/{variationId}", Handle)
            .WithTags("Cart")
            .RequireAuthorization(AuthorizationPoliciesConstant.RequireAuthenticated)
            .WithName("RemoveCartItem")
            .WithDisplayName("Remove Cart Item API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CartResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid variationId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new RemoveCartItemCommand(variationId), ct);

        return Results.Ok(ApiResponse<CartResponse>.Ok(response));
    }
}
