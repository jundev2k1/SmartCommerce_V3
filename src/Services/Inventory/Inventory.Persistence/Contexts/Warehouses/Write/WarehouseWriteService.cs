using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Persistence.Repository;
using Inventory.Application.Abstractions.Persistence.Warehouses;
using Inventory.Domain.ValueObjects;

namespace Inventory.Persistence.Contexts.Warehouses.Write;

public sealed class WarehouseWriteService(
    IRepository<Warehouse> repo,
    IUnitOfWork unitOfWork) : IWarehouseWriteService
{
    public async Task<Guid> CreateAsync(CreateWarehouseRequest request, CancellationToken ct = default)
    {
        var address = Address.Create(
            country: "US",
            stateOrProvince: "",
            city: request.Address,
            district: "",
            ward: "",
            street: request.Address,
            postalCode: "");

        var warehouse = Warehouse.Create(
            request.Code,
            request.Name,
            request.Type,
            address);

        await repo.AddAsync(warehouse, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return warehouse.Id;
    }
}
