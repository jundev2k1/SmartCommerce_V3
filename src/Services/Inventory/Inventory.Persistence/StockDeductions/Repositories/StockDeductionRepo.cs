namespace Inventory.Persistence.StockDeductions.Repositories;

public sealed class StockDeductionRepo(InventoryDbContext dbContext) : IStockDeductionRepository
{
    public async Task AddAsync(StockDeduction entity, CancellationToken ct = default)
    {
        await dbContext.StockDeductions.AddAsync(entity, ct);
    }
}
