using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;
using SmartEcommerce.BuildingBlock.SharedKernel.Extensions;

using SmartEcommerce.Order.Application.Features.Orders.Commands.CompleteOrder;

namespace SmartEcommerce.Order.API.Endpoints.Order;

public sealed class CompleteOrderEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Complete Order",
        "",
        "Marks a confirmed order as completed (fulfilled). Admin-only - only orders in Confirmed status can be completed.",
        "",
        "### Route Parameters",
        "- **orderId**: Unique identifier of the order (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Order not found",
        "- **400**: Order is not in Confirmed status",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders/{orderId}/complete", Handle)
            .WithTags("Order")
            .RequireAuthorization(AuthorizationPoliciesConstant.RequireAdmin)
            .WithName("CompleteOrder")
            .WithDisplayName("Complete Order API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CompleteOrderResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid orderId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new CompleteOrderCommand(orderId);
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<CompleteOrderResponse>.Ok(response));
    }
}
