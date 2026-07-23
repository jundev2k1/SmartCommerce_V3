using Inventory.Application.Abstractions.Persistence.InventoryTransactions;

namespace Inventory.Persistence.InventoryTransactions.Read;

public sealed class InventoryTransactionReadService(InventoryDbContext dbContext) : IInventoryTransactionReadService
{
    public async Task<IReadOnlyList<InventoryTransaction>> GetHistoryAsync(Guid inventoryId, CancellationToken ct = default)
    {
        return await dbContext.InventoryTransactions
            .AsNoTracking()
            .Where(t => t.InventoryId == inventoryId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }
}
