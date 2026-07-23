using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Persistence.InventoryTransactions.Repositories;

namespace Inventory.Persistence.InventoryTransactions.Write;

public sealed class InventoryTransactionWriteService(
    IInventoryTransactionRepository repo) : IInventoryTransactionWriteService
{
    public async Task StageAddAsync(InventoryTransaction entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);
    }
}
