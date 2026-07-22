using Inventory.Application.Abstractions.Repositories;

namespace Inventory.Persistence.Repository;

public sealed class InventoryTransactionRepo(InventoryDbContext dbContext) : IInventoryTransactionRepository
{
    public async Task AddAsync(InventoryTransaction entity, CancellationToken ct = default)
    {
        await dbContext.InventoryTransactions.AddAsync(entity, ct);
    }

    public async Task<IReadOnlyList<InventoryTransaction>> GetHistoryAsync(Guid inventoryId, CancellationToken ct = default)
    {
        return await dbContext.InventoryTransactions
            .AsNoTracking()
            .Where(t => t.InventoryId == inventoryId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }
}
