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

        await repo.AddAsync(entity, ct);
        await unitOfWork.ExecuteTransactionAsync(ct);
    }

    public async Task DeleteByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default)
    {
        var lots = await repo.GetByInventoryIdAsync(inventoryId, ct);
        foreach (var lot in lots)
        {
            repo.Remove(lot);
        }
        await unitOfWork.ExecuteTransactionAsync(ct);
    }
}
