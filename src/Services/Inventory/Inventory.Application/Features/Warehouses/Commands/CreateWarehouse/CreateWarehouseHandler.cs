using BuildingBlock.Application.Exceptions;

using Inventory.Application.Abstractions.Repositories;

namespace Inventory.Application.Features.Warehouses.Commands.CreateWarehouse;

public sealed class CreateWarehouseHandler(
    IWarehouseRepository warehouseRepo,
    IUnitOfWork uow) : ICommandHandler<CreateWarehouseCommand, CreateWarehouseResponse>
{
    public async Task<CreateWarehouseResponse> Handle(CreateWarehouseCommand request, CancellationToken ct = default)
    {
        var existing = await warehouseRepo.GetByCodeAsync(request.Code, ct);
        if (existing is not null)
            throw new ConflictException($"Warehouse with code ({request.Code}) already exists");

        var warehouse = Warehouse.Create(
            Guid.CreateVersion7(),
            request.Code.Trim(),
            request.Name.Trim(),
            request.Address.Trim());
        await warehouseRepo.AddAsync(warehouse, ct);
        await uow.SaveChangesAsync(ct);

        return new CreateWarehouseResponse(warehouse.Id);
    }
}
