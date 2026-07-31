using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Application.Features.Inventories.DTOs;
using Inventory.Persistence.Contexts.InventoryTransactions.Repositories;

namespace Inventory.Persistence.Contexts.InventoryTransactions.Write;

public sealed class InventoryTransactionWriteService(
    IInventoryTransactionRepository repo) : IInventoryTransactionWriteService
{
    public async Task StageAddAsync(CreateInventoryTransactionDto request, CancellationToken ct = default)
    {
        var entity = InventoryTransaction.Create(
            request.InventoryId,
            request.ProductId,
            request.VariationId,
            request.WarehouseId,
            request.Type,
            request.Quantity,
            request.QuantityBefore,
            request.QuantityAfter,
            request.Reason);

        await repo.AddAsync(entity, ct);
    }
}
