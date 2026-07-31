using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Persistence.Contexts.InventoryTransactions.Repositories;

namespace Inventory.Persistence.Contexts.InventoryTransactions.Write;

public sealed class InventoryTransactionWriteService(
    IInventoryTransactionRepository repo) : IInventoryTransactionWriteService
{
    public async Task StageAddAsync(CreateInventoryTransactionRequest request, CancellationToken ct = default)
    {
        var entity = InventoryTransaction.Create(
            request.InventoryId,
            request.ProductId,
            request.VariationId,
            request.WarehouseId,
            request.Type,
            request.Quantity,
            request.QuantityAfter,
            request.Reason);

        await repo.AddAsync(entity, ct);
    }
}
