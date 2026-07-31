using BuildingBlock.Persistence.Repository;

using Inventory.Application.Abstractions.Persistence.StockDeductions;

namespace Inventory.Persistence.Contexts.StockDeductions.Write;

/// <summary>Both methods are non-committing - see Correction 2 in the persistence refactor tracker.</summary>
public sealed class StockDeductionWriteService(
    IRepository<StockDeduction, Guid> repo) : IStockDeductionWriteService
{
    public async Task StageAddAsync(StockDeduction entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);
    }

    public async Task StageUpdateAsync(Guid deductionId, Func<StockDeduction, Task> updateAction, CancellationToken ct = default)
    {
        await repo.UpdateAsync(deductionId, updateAction, ct);
    }
}
