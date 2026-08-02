using BuildingBlock.Application.Abstractions.Persistence;
using Inventory.Application.Abstractions.Persistence.InventorySerials;
using Inventory.Persistence.Contexts.InventorySerials.Repositories;
using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Contexts.InventorySerials.Write;

public sealed class InventorySerialWriteService(
    IInventorySerialRepository repo,
    IUnitOfWork unitOfWork) : IInventorySerialWriteService
{
    public async Task AddAsync(CreateInventorySerialRequest request, CancellationToken ct = default)
    {
        var entity = InventorySerial.Create(request.InventoryId, request.SerialNumber);

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await repo.AddAsync(entity, ct);
        }, ct: ct);
    }

    public async Task DeleteByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await repo.DeleteWithNoTrackingAsync(s => s.InventoryId == inventoryId, ct);
        }, ct: ct);
    }
}
