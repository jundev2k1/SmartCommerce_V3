using BuildingBlock.Application.Abstractions.Persistence;
using Inventory.Application.Abstractions.Persistence.InventoryLots;
using Inventory.Persistence.Contexts.InventoryLots.Repositories;
using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Contexts.InventoryLots.Write;

public sealed class InventoryLotWriteService(
    IInventoryLotRepository repo,
    IUnitOfWork unitOfWork) : IInventoryLotWriteService
{
    public async Task AddAsync(CreateInventoryLotRequest request, CancellationToken ct = default)
    {
        var entity = InventoryLot.Create(
            request.InventoryId,
            request.LotNumber,
            request.ManufactureDate,
            request.ExpiredDate,
            request.Quantity,
            request.SupplierLotNumber,
            request.CountryOfOrigin);

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await repo.AddAsync(entity, ct);
        }, ct: ct);
    }

    public async Task DeleteByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await repo.DeleteWithNoTrackingAsync(l => l.InventoryId == inventoryId, ct);
        }, ct: ct);
    }
}
