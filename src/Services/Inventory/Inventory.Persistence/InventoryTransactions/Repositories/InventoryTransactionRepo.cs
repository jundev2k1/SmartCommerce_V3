namespace Inventory.Persistence.InventoryTransactions.Repositories;

public sealed class InventoryTransactionRepo(InventoryDbContext dbContext) : IInventoryTransactionRepository
{
    public async Task AddAsync(InventoryTransaction entity, CancellationToken ct = default)
    {
        await dbContext.InventoryTransactions.AddAsync(entity, ct);
    }
}
