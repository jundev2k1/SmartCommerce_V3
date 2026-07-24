using BuildingBlock.Application.Exceptions;
using BuildingBlock.Persistence.Repository;

namespace Inventory.Persistence.StockDeductions.Repositories;

public sealed class StockDeductionRepo(InventoryDbContext dbContext) : IStockDeductionRepository, IRepository<StockDeduction>
{
    public async Task<StockDeduction?> GetByIdAsync<TId>(TId id, CancellationToken ct = default)
    {
        return await dbContext.StockDeductions
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id.Equals(id), ct);
    }

    public async Task<StockDeduction?> GetByIdAsync<TId>(
        TId id,
        Func<IQueryable<StockDeduction>, IQueryable<StockDeduction>> includes,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.StockDeductions);
        return await query.FirstOrDefaultAsync(d => d.Id.Equals(id), ct);
    }

    public async Task AddAsync(StockDeduction entity, CancellationToken ct = default)
    {
        await dbContext.StockDeductions.AddAsync(entity, ct);
    }

    public async Task AddRangeAsync(IEnumerable<StockDeduction> entities, CancellationToken ct = default)
    {
        await dbContext.StockDeductions.AddRangeAsync(entities, ct);
    }

    public async Task UpdateAsync<TId>(TId id, Func<StockDeduction, Task> updateAction, CancellationToken ct = default)
    {
        var deduction = await dbContext.StockDeductions
            .FirstOrDefaultAsync(d => d.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(StockDeduction), id!);

        await updateAction(deduction);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<StockDeduction>, IQueryable<StockDeduction>> includes,
        Func<StockDeduction, Task> updateAction,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.StockDeductions);
        var deduction = await query.FirstOrDefaultAsync(d => d.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(StockDeduction), id!);

        await updateAction(deduction);
    }

    public async Task DeleteAsync<TId>(TId id, CancellationToken ct = default)
    {
        var deduction = await dbContext.StockDeductions
            .FirstOrDefaultAsync(d => d.Id!.Equals(id), ct);

        if (deduction is not null)
            dbContext.StockDeductions.Remove(deduction);
    }

    public async Task DeleteRangeAsync<TId>(TId[] ids, CancellationToken ct = default)
    {
        await dbContext.StockDeductions
            .Where(d => ids.Any(id => id!.Equals(d.Id)))
            .ExecuteDeleteAsync(ct);
    }
}
