using BuildingBlock.Application.Exceptions;
using BuildingBlock.Persistence.Repository;

using Order.Persistence.Engine;

namespace Order.Persistence.Contexts.Orders.Repositories;

public sealed class OrderRepo(OrderDbContext dbContext) : IOrderRepository, IRepository<OrderEntity>
{
    // Items/Owner are now real (non-owned) relationships, so unlike before they need an explicit
    // Include - see OrderReadService for the same reasoning, applied here for IRepository<T>
    // callers (OrderWriteService.UpdateItemsAsync/ConfirmAsync/CancelAsync/CompleteAsync all read
    // Items/TotalAmount or Owner.CustomerId).
    public async Task<OrderEntity?> GetByIdAsync<TId>(TId id, CancellationToken ct = default)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Owner)
            .FirstOrDefaultAsync(o => o.Id.Equals(id), ct);
    }

    public async Task<OrderEntity?> GetByIdAsync<TId>(
        TId id,
        Func<IQueryable<OrderEntity>, IQueryable<OrderEntity>> includes,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.Orders);
        return await query.FirstOrDefaultAsync(o => o.Id.Equals(id), ct);
    }

    public async Task AddAsync(OrderEntity entity, CancellationToken ct = default)
    {
        await dbContext.Orders.AddAsync(entity, ct);
    }

    public async Task AddRangeAsync(IEnumerable<OrderEntity> entities, CancellationToken ct = default)
    {
        await dbContext.Orders.AddRangeAsync(entities, ct);
    }

    public async Task UpdateAsync<TId>(TId id, Func<OrderEntity, Task> updateAction, CancellationToken ct = default)
    {
        var order = await dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.Owner)
            .FirstOrDefaultAsync(o => o.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(OrderEntity), id!);

        await updateAction(order);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<OrderEntity>, IQueryable<OrderEntity>> includes,
        Func<OrderEntity, Task> updateAction,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.Orders);
        var order = await query.FirstOrDefaultAsync(o => o.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(OrderEntity), id!);

        await updateAction(order);
    }

    public async Task DeleteAsync<TId>(TId id, CancellationToken ct = default)
    {
        var order = await dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id!.Equals(id), ct);

        if (order is not null)
            dbContext.Orders.Remove(order);
    }

    public async Task DeleteRangeAsync<TId>(TId[] ids, CancellationToken ct = default)
    {
        await dbContext.Orders
            .Where(o => ids.Any(id => id!.Equals(o.Id)))
            .ExecuteDeleteAsync(ct);
    }
}
