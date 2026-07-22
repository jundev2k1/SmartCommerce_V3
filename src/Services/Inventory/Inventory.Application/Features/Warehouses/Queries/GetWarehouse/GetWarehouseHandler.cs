using BuildingBlock.Application.Exceptions;

using Inventory.Application.Abstractions.Repositories;

using Mapster;

namespace Inventory.Application.Features.Warehouses.Queries.GetWarehouse;

public sealed class GetWarehouseHandler(IWarehouseRepository warehouseRepo)
    : IQueryHandler<GetWarehouseQuery, GetWarehouseResponse>
{
    public async Task<GetWarehouseResponse> Handle(GetWarehouseQuery request, CancellationToken ct = default)
    {
        var warehouse = await warehouseRepo.GetByIdAsync(request.WarehouseId, ct)
            ?? throw new NotFoundException("Warehouse", request.WarehouseId);

        return warehouse.Adapt<GetWarehouseResponse>();
    }
}
