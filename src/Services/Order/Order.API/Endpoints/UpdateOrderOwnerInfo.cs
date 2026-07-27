using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.Infrastructure.Idempotency;
using BuildingBlock.SharedKernel.Constants;
using BuildingBlock.SharedKernel.Extensions;
using BuildingBlock.Web.Swagger.EndpointHeader;

using Order.Application.Features.Orders.Commands.UpdateOrderOwnerInfo;

namespace Order.API.Endpoints;

public sealed record UpdateOrderOwnerInfoRequest(string CustomerPhone, string ShippingAddress);

public sealed class UpdateOrderOwnerInfoEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Update Order Owner Information",
        "",
        "Updates the shipping address and contact phone number snapshot on an order. Only the",
        "order's own customer or an admin may call this. Allowed while the order is Pending or",
        "Confirmed - a Cancelled/Completed order can no longer have its shipping info changed.",
        "",
        "### Route Parameters",
        "- **orderId**: Unique identifier of the order (required, must be valid GUID)",
        "",
        "### Request Body",
        "- **CustomerPhone**: New contact phone number (required)",
        "- **ShippingAddress**: New shipping address (required)",
        "",
        "### Error Responses",
        "- **404**: Order not found",
        "- **403**: Caller is neither the order's owner nor an admin",
        "- **400**: Order is Cancelled or Completed",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/orders/{orderId}/owner-info", Handle)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .Headers([
                new HeaderDefinition(HeaderKeys.IdempotencyKey, true, "Ensures a retried update is applied only once")
            ])
            .RequireIdempotency()
            .WithName("UpdateOrderOwnerInfo")
            .WithDisplayName("Update Order Owner Info API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<UpdateOrderOwnerInfoResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid orderId,
        [FromBody] UpdateOrderOwnerInfoRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new UpdateOrderOwnerInfoCommand(
            orderId,
            request.CustomerPhone.Trim(),
            request.ShippingAddress.Trim());
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<UpdateOrderOwnerInfoResponse>.Ok(response));
    }
}
