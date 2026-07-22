namespace Inventory.Application.Features.Warehouses.Commands.CreateWarehouse;

public sealed record CreateWarehouseCommand(
    string Code,
    string Name,
    string Address) : ICommand<CreateWarehouseResponse>;

public sealed record CreateWarehouseResponse(Guid WarehouseId);
