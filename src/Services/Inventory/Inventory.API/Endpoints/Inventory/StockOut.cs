using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Application.Authorization;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;
using SmartEcommerce.BuildingBlock.SharedKernel.Extensions;

using SmartEcommerce.Inventory.Application.Features.Inventories.Commands.StockOut;

namespace SmartEcommerce.Inventory.API.Endpoints.Inventory;

public sealed record StockOutRequest(int Quantity, string Reason);

public sealed class StockOutEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Stock Out",
        "",
        "Decreases the stock quantity of an inventory record and records a StockOut transaction.",
        "",
        "### Route Parameters",
        "- **inventoryId**: Unique identifier of the inventory record (required, must be valid GUID)",
        "",
        "### Request Body",
        "- **Quantity**: Amount to remove (required, must be greater than 0)",
        "- **Reason**: Reason for the stock movement (required)",
        "",
        "### Error Responses",
        "- **404**: Inventory not found",
        "- **400**: Invalid request, validation failed, or insufficient stock",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/inventories/{inventoryId}/stock-out", Handle)
            .WithTags("Inventory")
            .RequirePermissions(Permissions.Inventory.Manage)
            .WithName("StockOut")
            .WithDisplayName("Stock Out API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<StockOutResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid inventoryId,
        [FromBody] StockOutRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new StockOutCommand(inventoryId, request.Quantity, request.Reason.Trim());
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<StockOutResponse>.Ok(response));
    }
}
