using BuildingBlock.Persistence.Repository;

using Inventory.Application.Abstractions.Persistence.StockDeductions;

namespace Inventory.Persistence.Contexts.StockDeductions.Write;

/// <summary>Both methods are non-committing - see Correction 2 in the persistence refactor tracker.</summary>
public sealed class StockDeductionWriteService(
    IRepository<StockDeduction, Guid> repo) : IStockDeductionWriteService
{
    public async Task StageFailedAsync(Guid deductionId, string failureCode, string? reason, CancellationToken ct = default)
    {
        await repo.AddAsync(StockDeduction.CreateFailed(deductionId, failureCode, reason), ct);
    }

    public async Task StageSucceededAsync(Guid deductionId, string itemsJson, string? reason, CancellationToken ct = default)
    {
        await repo.AddAsync(StockDeduction.CreateSucceeded(deductionId, itemsJson, reason), ct);
    }

    public async Task MarkReversedAsync(Guid deductionId, CancellationToken ct = default)
    {
        await repo.UpdateAsync(deductionId, d => d.MarkReversed(), ct);
    }
}
