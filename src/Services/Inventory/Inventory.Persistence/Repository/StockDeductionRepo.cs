using BuildingBlock.Application.Exceptions;

using Inventory.Application.Abstractions.Repositories;

namespace Inventory.Persistence.Repository;

public sealed class StockDeductionRepo(InventoryDbContext dbContext) : IStockDeductionRepository
{
    public async Task<StockDeduction?> GetByIdAsync(Guid deductionId, CancellationToken ct = default)
    {
        return await dbContext.StockDeductions
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == deductionId, ct);
    }

    public async Task AddAsync(StockDeduction entity, CancellationToken ct = default)
    {
        await dbContext.StockDeductions.AddAsync(entity, ct);
    }

    public async Task UpdateAsync(
        Guid deductionId,
        Func<StockDeduction, Task> updateAction,
        CancellationToken ct = default)
    {
        var deduction = await dbContext.StockDeductions
            .FirstOrDefaultAsync(d => d.Id == deductionId, ct)
            ?? throw new NotFoundException(nameof(StockDeduction), deductionId);

        await updateAction(deduction);
    }
}
