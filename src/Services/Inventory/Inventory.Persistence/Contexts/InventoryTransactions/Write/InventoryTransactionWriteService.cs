using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Persistence.Contexts.InventoryTransactions.Repositories;

namespace Inventory.Persistence.Contexts.InventoryTransactions.Write;

public sealed class InventoryTransactionWriteService(
    IInventoryTransactionRepository repo) : IInventoryTransactionWriteService
{
    public async Task StageAddAsync(InventoryTransaction entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);
    }
}
