using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Extensions;

using Order.Application.Features.Orders.Commands.AdminCreateOrder;
using Order.Application.Features.Orders.Common;

namespace Order.API.Endpoints;

public sealed record AdminCreateOrderItemRequestDto(Guid ProductId, Guid VariationId, int Quantity, decimal Discount = 0m);

public sealed record AdminCreateOrderRequest(
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    string ShippingAddress,
    IReadOnlyCollection<AdminCreateOrderItemRequestDto> Items);

public sealed class AdminCreateOrderEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Admin Create Order",
        "",
        "Admin/Root only. Creates an order on behalf of an explicitly specified customer - unlike",
        "POST /orders, there's no \"current user's cart\" to source or diff against, so items are",
        "taken as given (still validated against the product catalog and stock, same as the client",
        "path) and the customer's cart is never touched.",
        "",
        "### Request Body",
        "- **CustomerId**: Id of the customer the order is being created for (required)",
        "- **CustomerName**: Snapshot of the customer's display name, captured once and never resynced (required)",
        "- **CustomerPhone**: Snapshot of the customer's phone number, captured once and never resynced (required)",
        "- **ShippingAddress**: Free-text shipping address, same snapshot convention as CustomerName/CustomerPhone (required)",
        "- **Items**: List of order items (required, at least one item, max 50, no duplicates)",
        "  - **ProductId**: Id of the product (required)",
        "  - **VariationId**: Id of the product variation being ordered (required, must exist and be Active in the local product catalog)",
        "  - **Quantity**: Quantity requested (required, 1-100)",
        "  - **Discount**: Optional flat discount applied to the line (default 0, cannot exceed the line's pre-discount total)",
        "",
        "### Error Responses",
        "- **400**: Invalid request, validation failed, a product is not orderable, or stock is insufficient",
        "- **403**: Caller is not Admin/Root",
        "- **404**: Product not found in the local product catalog",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders/admin", Handle)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("AdminCreateOrder")
            .WithDisplayName("Admin Create Order API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreateOrderResponse>>(StatusCodes.Status202Accepted);
    }

    private static async Task<IResult> Handle(
        [FromBody] AdminCreateOrderRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new AdminCreateOrderCommand(
            request.CustomerId,
            request.CustomerName,
            request.CustomerPhone,
            request.ShippingAddress,
            [.. request.Items.Select(i => new OrderItemRequest(i.ProductId, i.VariationId, i.Quantity, i.Discount))]);

        var response = await sender.Send(command, ct);

        return Results.Accepted($"/orders/{response.OrderId}",
            ApiResponse<CreateOrderResponse>.Ok(response));
    }
}
