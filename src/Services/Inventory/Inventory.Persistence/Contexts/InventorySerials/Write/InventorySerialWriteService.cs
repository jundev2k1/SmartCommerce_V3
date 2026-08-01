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

        await repo.AddAsync(entity, ct);
        await unitOfWork.ExecuteTransactionAsync(ct);
    }

    public async Task DeleteByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default)
    {
        var serials = await repo.GetByInventoryIdAsync(inventoryId, ct);
        foreach (var serial in serials)
        {
            repo.Remove(serial);
        }
        await unitOfWork.ExecuteTransactionAsync(ct);
    }
}
