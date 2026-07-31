namespace Inventory.Application.Abstractions.Persistence.StockDeductions;

public interface IStockDeductionWriteService
{
    /// <summary>Non-committing - see Correction 2 in the persistence refactor tracker.</summary>
    Task StageFailedAsync(Guid deductionId, string failureCode, string? reason, CancellationToken ct = default);

    /// <summary>Non-committing - see Correction 2 in the persistence refactor tracker.</summary>
    Task StageSucceededAsync(Guid deductionId, string itemsJson, string? reason, CancellationToken ct = default);

    /// <summary>Non-committing - see Correction 2 in the persistence refactor tracker.</summary>
    Task MarkReversedAsync(Guid deductionId, CancellationToken ct = default);
}
