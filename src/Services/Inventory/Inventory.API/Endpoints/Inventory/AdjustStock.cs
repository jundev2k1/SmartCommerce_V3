using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;
using SmartEcommerce.BuildingBlock.SharedKernel.Extensions;

using SmartEcommerce.Inventory.Application.Features.Inventories.Commands.AdjustStock;

namespace SmartEcommerce.Inventory.API.Endpoints.Inventory;

public sealed record AdjustStockRequest(int NewQuantity, string Reason);

public sealed class AdjustStockEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Adjust Stock",
        "",
        "Directly corrects the stock quantity of an inventory record (e.g. after a physical count)",
        "and records an Adjustment transaction.",
        "",
        "### Route Parameters",
        "- **inventoryId**: Unique identifier of the inventory record (required, must be valid GUID)",
        "",
        "### Request Body",
        "- **NewQuantity**: Corrected stock quantity (required, cannot be negative)",
        "- **Reason**: Reason for the adjustment (required)",
        "",
        "### Error Responses",
        "- **404**: Inventory not found",
        "- **400**: Invalid request or validation failed",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/inventories/{inventoryId}/adjust", Handle)
            .WithTags("Inventory")
            .RequireAuthorization(AuthorizationPoliciesConstant.RequireAdmin)
            .WithName("AdjustStock")
            .WithDisplayName("Adjust Stock API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<AdjustStockResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid inventoryId,
        [FromBody] AdjustStockRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new AdjustStockCommand(inventoryId, request.NewQuantity, request.Reason.Trim());
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<AdjustStockResponse>.Ok(response));
    }
}
