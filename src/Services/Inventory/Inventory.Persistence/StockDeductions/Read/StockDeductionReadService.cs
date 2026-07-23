using Inventory.Application.Abstractions.Persistence.StockDeductions;

namespace Inventory.Persistence.StockDeductions.Read;

public sealed class StockDeductionReadService(InventoryDbContext dbContext) : IStockDeductionReadService
{
    public async Task<StockDeduction?> GetByIdAsync(Guid deductionId, CancellationToken ct = default)
    {
        return await dbContext.StockDeductions
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == deductionId, ct);
    }
}
