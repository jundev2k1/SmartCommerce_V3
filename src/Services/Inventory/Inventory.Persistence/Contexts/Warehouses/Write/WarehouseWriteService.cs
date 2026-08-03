using SmartEcommerce.BuildingBlock.Application.Abstractions.Persistence;
using SmartEcommerce.BuildingBlock.Persistence.Repository;
using SmartEcommerce.Inventory.Application.Abstractions.Persistence.Warehouses;
using SmartEcommerce.Inventory.Domain.ValueObjects;

namespace SmartEcommerce.Inventory.Persistence.Contexts.Warehouses.Write;

public sealed class WarehouseWriteService(
    IRepository<Warehouse> repo,
    IUnitOfWork unitOfWork) : IWarehouseWriteService
{
    public async Task<Guid> CreateAsync(CreateWarehouseRequest request, CancellationToken ct = default)
    {
        var address = Address.Create(
            country: request.Country,
            stateOrProvince: request.StateOrProvince,
            city: request.City,
            district: request.District,
            ward: request.Ward,
            street: request.Street,
            postalCode: request.PostalCode);

        var warehouse = Warehouse.Create(
            request.Code,
            request.Name,
            request.Type,
            address);

        warehouse.AddZone(
            code: "DEFAULT",
            name: "Default Storage Zone",
            type: WarehouseZoneType.Storage,
            priority: 0,
            capacity: null,
            temperature: null,
            humidity: null,
            pickingStrategy: PickingStrategy.FIFO,
            allowMixedLot: false);

        await repo.AddAsync(warehouse, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return warehouse.Id;
    }
}
