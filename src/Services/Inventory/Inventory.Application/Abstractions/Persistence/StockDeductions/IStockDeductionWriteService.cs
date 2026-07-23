namespace Inventory.Application.Abstractions.Persistence.StockDeductions;

public interface IStockDeductionWriteService
{
    /// <summary>Non-committing - see Correction 2 in the persistence refactor tracker.</summary>
    Task StageAddAsync(StockDeduction entity, CancellationToken ct = default);

    /// <summary>Non-committing - see Correction 2 in the persistence refactor tracker.</summary>
    Task StageUpdateAsync(Guid deductionId, Func<StockDeduction, Task> updateAction, CancellationToken ct = default);
}
