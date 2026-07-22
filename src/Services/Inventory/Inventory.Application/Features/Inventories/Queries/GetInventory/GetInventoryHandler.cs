using BuildingBlock.Application.Exceptions;

using Inventory.Application.Abstractions.Repositories;

using Mapster;

namespace Inventory.Application.Features.Inventories.Queries.GetInventory;

public sealed class GetInventoryHandler(IInventoryRepository inventoryRepo)
    : IQueryHandler<GetInventoryQuery, GetInventoryResponse>
{
    public async Task<GetInventoryResponse> Handle(GetInventoryQuery request, CancellationToken ct = default)
    {
        var inventory = await inventoryRepo.GetByIdAsync(request.InventoryId, ct)
            ?? throw new NotFoundException("Inventory", request.InventoryId);

        return inventory.Adapt<GetInventoryResponse>();
    }
}
