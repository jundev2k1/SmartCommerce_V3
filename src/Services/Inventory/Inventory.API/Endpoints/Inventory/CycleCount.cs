using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Extensions;

using Inventory.Application.Features.Inventories.Commands.CycleCount;

namespace Inventory.API.Endpoints.Inventory;

// Start Cycle Count Endpoint
public sealed record StartCycleCountRequest(
    string WarehouseId,
    string CountDate,
    string Description);

public sealed class StartCycleCountEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Start Cycle Count",
        "",
        "Initiates a new physical inventory count for a warehouse.",
        "Creates a cycle count document to track the counting process.",
        "",
        "### Request Body",
        "- **WarehouseId**: Warehouse to count (required, must be valid GUID)",
        "- **CountDate**: Date of physical count (required, ISO 8601 format)",
        "- **Description**: Reason/notes for count (required)",
        "",
        "### Response",
        "- **CountId**: Unique cycle count identifier",
        "- **CountNumber**: Count reference number",
        "- **Status**: Current count status (Draft)",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/inventories/cycle-count/start", Handle)
            .WithTags("Inventory")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("StartCycleCount")
            .WithDisplayName("Start Cycle Count API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<StartCycleCountResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromBody] StartCycleCountRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new StartCycleCountCommand(
            WarehouseId: Guid.Parse(request.WarehouseId),
            CountDate: request.CountDate,
            Description: request.Description.Trim());

        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<StartCycleCountResponse>.Ok(response));
    }
}

// Complete Cycle Count Endpoint
public sealed record CountedItemRequest(
    string ProductVariantId,
    int ActualQuantity);

public sealed record CompleteCycleCountRequest(
    string CountId,
    IReadOnlyList<CountedItemRequest> CountedItems,
    decimal VarianceThresholdPercent = 5m);

public sealed class CompleteCycleCountEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Complete Cycle Count",
        "",
        "Finalizes a physical count, calculates variances, and auto-adjusts stock.",
        "Automatically creates adjustment documents for variances exceeding threshold.",
        "",
        "### Request Body",
        "- **CountId**: Cycle count ID from start operation (required, must be valid GUID)",
        "- **CountedItems**: Array of counted items (required, minimum 1 item)",
        "  - **ProductVariantId**: Variant counted (required, must be valid GUID)",
        "  - **ActualQuantity**: Physical quantity counted (required, >= 0)",
        "- **VarianceThresholdPercent**: Min variance % to trigger adjustment (optional, default 5%)",
        "",
        "### Response",
        "- **TotalItemsCounted**: Number of items counted",
        "- **ItemsWithVariance**: Items where actual ≠ expected",
        "- **ItemsAdjusted**: Automatic adjustments created",
        "- **TotalVarianceValue**: Sum of all variances",
        "- **Variances**: Detailed variance breakdown per item",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/inventories/cycle-count/complete", Handle)
            .WithTags("Inventory")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("CompleteCycleCount")
            .WithDisplayName("Complete Cycle Count API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CompleteCycleCountResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromBody] CompleteCycleCountRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var items = request.CountedItems
            .Select(i => new CycleCountItemRequest(
                ProductVariantId: Guid.Parse(i.ProductVariantId),
                ActualQuantity: i.ActualQuantity))
            .ToList();

        var command = new CompleteCycleCountCommand(
            CountId: Guid.Parse(request.CountId),
            CountedItems: items,
            VarianceThresholdPercent: request.VarianceThresholdPercent);

        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<CompleteCycleCountResponse>.Ok(response));
    }
}
