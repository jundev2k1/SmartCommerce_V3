using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Extensions;

using Order.Application.Features.Orders.Commands.UpdateOrder;
using Order.Application.Features.Orders.Common;

namespace Order.API.Endpoints;

public sealed record UpdateOrderItemRequestDto(Guid ProductId, Guid VariationId, int Quantity, decimal Discount = 0m);

public sealed record UpdateOrderRequest(IReadOnlyCollection<UpdateOrderItemRequestDto> Items);

public sealed class UpdateOrderEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Update Order",
        "",
        "Replaces the item list of a Pending order. Re-validates every item against the synced",
        "product catalog and current stock availability, same as CreateOrder. Orders that have",
        "already left the Pending status (Confirmed/Cancelled/Completed) cannot be updated.",
        "",
        "### Route Parameters",
        "- **orderId**: Unique identifier of the order (required, must be valid GUID)",
        "",
        "### Request Body",
        "- **Items**: List of order items (required, at least one item, max 50, no duplicates)",
        "",
        "### Error Responses",
        "- **404**: Order not found",
        "- **400**: Order is not in Pending status, a product is not orderable, or stock is insufficient",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/orders/{orderId}", Handle)
            .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated)
            .WithName("UpdateOrder")
            .WithDisplayName("Update Order API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<UpdateOrderResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid orderId,
        [FromBody] UpdateOrderRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new UpdateOrderCommand(
            orderId,
            [.. request.Items.Select(i => new OrderItemRequest(i.ProductId, i.VariationId, i.Quantity, i.Discount))]);

        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<UpdateOrderResponse>.Ok(response));
    }
}
