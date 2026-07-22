namespace Inventory.Application.Abstractions.Repositories;

public interface IStockDeductionRepository
{
    Task<StockDeduction?> GetByIdAsync(Guid deductionId, CancellationToken ct = default);

    Task AddAsync(StockDeduction entity, CancellationToken ct = default);

    Task UpdateAsync(
        Guid deductionId,
        Func<StockDeduction, Task> updateAction,
        CancellationToken ct = default);
}
