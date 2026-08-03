using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Constants;
using BuildingBlock.SharedKernel.Extensions;

using Order.Application.Features.Orders.Commands.CancelOrder;

namespace Order.API.Endpoints.Order;

public sealed class CancelOrderEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Cancel Order",
        "",
        "Cancels a pending order. Orders that are already cancelled or completed cannot be cancelled.",
        "",
        "### Route Parameters",
        "- **orderId**: Unique identifier of the order (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Order not found",
        "- **400**: Order is already cancelled or completed",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders/{orderId}/cancel", Handle)
            .WithTags("Order")
            .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated)
            .WithName("CancelOrder")
            .WithDisplayName("Cancel Order API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CancelOrderResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid orderId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new CancelOrderCommand(orderId);
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<CancelOrderResponse>.Ok(response));
    }
}
