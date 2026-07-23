namespace Inventory.Application.Abstractions.Persistence.StockDeductions;

public interface IStockDeductionReadService
{
    Task<StockDeduction?> GetByIdAsync(Guid deductionId, CancellationToken ct = default);
}
