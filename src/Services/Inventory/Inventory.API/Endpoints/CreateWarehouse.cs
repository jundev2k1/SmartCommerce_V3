using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Extensions;

using Inventory.Application.Features.Warehouses.Commands.CreateWarehouse;

namespace Inventory.API.Endpoints;

public sealed record CreateWarehouseRequest(
    string Code,
    string Name,
    string Address);

public sealed class CreateWarehouseEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Create Warehouse",
        "",
        "Creates a new warehouse location.",
        "",
        "### Request Body",
        "- **Code**: Unique warehouse code (required, must be unique)",
        "- **Name**: Warehouse name (required)",
        "- **Address**: Warehouse address (required)",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
        "- **409**: Warehouse code already exists",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/warehouses", Handle)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("CreateWarehouse")
            .WithDisplayName("Create Warehouse API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreateWarehouseResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateWarehouseRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new CreateWarehouseCommand(
            request.Code.Trim(),
            request.Name.Trim(),
            request.Address.Trim());

        var response = await sender.Send(command, ct);

        return Results.Created($"/warehouses/{response.WarehouseId}",
            ApiResponse<CreateWarehouseResponse>.Ok(response));
    }
}
