using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Constants;
using BuildingBlock.SharedKernel.Extensions;

using Inventory.Application.Features.Inventories.Commands.StockIn;

namespace Inventory.API.Endpoints.Inventory;

public sealed record StockInRequest(int Quantity, string Reason);

public sealed class StockInEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Stock In",
        "",
        "Increases the stock quantity of an inventory record and records a StockIn transaction.",
        "",
        "### Route Parameters",
        "- **inventoryId**: Unique identifier of the inventory record (required, must be valid GUID)",
        "",
        "### Request Body",
        "- **Quantity**: Amount to add (required, must be greater than 0)",
        "- **Reason**: Reason for the stock movement (required)",
        "",
        "### Error Responses",
        "- **404**: Inventory not found",
        "- **400**: Invalid request or validation failed",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/inventories/{inventoryId}/stock-in", Handle)
            .WithTags("Inventory")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("StockIn")
            .WithDisplayName("Stock In API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<StockInResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid inventoryId,
        [FromBody] StockInRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new StockInCommand(inventoryId, request.Quantity, request.Reason.Trim());
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<StockInResponse>.Ok(response));
    }
}
