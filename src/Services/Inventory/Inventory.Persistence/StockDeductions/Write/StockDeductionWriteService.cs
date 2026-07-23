using BuildingBlock.Application.Exceptions;

using Inventory.Application.Abstractions.Persistence.StockDeductions;
using Inventory.Persistence.StockDeductions.Repositories;

namespace Inventory.Persistence.StockDeductions.Write;

public sealed class StockDeductionWriteService(
    InventoryDbContext dbContext,
    IStockDeductionRepository repo) : IStockDeductionWriteService
{
    public async Task StageAddAsync(StockDeduction entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);
    }

    public async Task StageUpdateAsync(Guid deductionId, Func<StockDeduction, Task> updateAction, CancellationToken ct = default)
    {
        var deduction = await dbContext.StockDeductions
            .FirstOrDefaultAsync(d => d.Id == deductionId, ct)
            ?? throw new NotFoundException(nameof(StockDeduction), deductionId);

        await updateAction(deduction);
    }
}
