using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;
using SmartEcommerce.BuildingBlock.SharedKernel.Extensions;

using SmartEcommerce.Order.Application.Abstractions.Services;
using SmartEcommerce.Order.Application.Features.Cart.Commands.UpdateCartItemQuantity;

namespace SmartEcommerce.Order.API.Endpoints.Cart;

public sealed record UpdateCartItemQuantityRequest(int Quantity);

public sealed class UpdateCartItemQuantityEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Update Cart Item Quantity",
        "",
        "Sets a cart line to an absolute Quantity. Rejects Quantity <= 0 - use the remove-item",
        "endpoint to delete a line instead of zeroing it out.",
        "",
        "### Route Parameters",
        "- **variationId**: Id of the product variation already in the cart (required)",
        "",
        "### Request Body",
        "- **Quantity**: New absolute quantity for the line (required, greater than 0)",
        "",
        "### Error Responses",
        "- **404**: Variation not found in the cart",
        "- **400**: Quantity is 0 or negative, or exceeds real-time Inventory stock - the latter's",
        "  `details` carries `{ insufficients: [variationId] }`",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/cart/items/{variationId}", Handle)
            .WithTags("Cart")
            .RequireAuthorization(AuthorizationPoliciesConstant.RequireAuthenticated)
            .WithName("UpdateCartItemQuantity")
            .WithDisplayName("Update Cart Item Quantity API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CartResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid variationId,
        [FromBody] UpdateCartItemQuantityRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new UpdateCartItemQuantityCommand(variationId, request.Quantity);
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<CartResponse>.Ok(response));
    }
}
