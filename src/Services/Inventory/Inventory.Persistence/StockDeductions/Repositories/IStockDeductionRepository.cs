namespace Inventory.Persistence.StockDeductions.Repositories;

public interface IStockDeductionRepository
{
    Task AddAsync(StockDeduction entity, CancellationToken ct = default);
}
