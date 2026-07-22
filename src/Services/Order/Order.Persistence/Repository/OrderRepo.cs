using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Persistence.Ef.Criteria;
using BuildingBlock.Persistence.Repository;

using Order.Application.Abstractions.Repositories;
using Order.Application.Features.Orders.Search;

namespace Order.Persistence.Repository;

public sealed class OrderRepo(OrderDbContext dbContext) : IOrderRepository, IRepository<OrderEntity>
{
    public async Task<OrderEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<PaginatedResult<OrderEntity>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .ApplyCriteria(OrderCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }

    public async Task<OrderEntity?> GetByIdAsync(
        Guid id,
        Func<IQueryable<OrderEntity>, IQueryable<OrderEntity>> includes,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.Orders);
        return await query.FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<OrderEntity?> GetByIdempotencyKeyAsync(Guid customerId, string idempotencyKey, CancellationToken ct = default)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.IdempotencyKey == idempotencyKey, ct);
    }

    public async Task AddAsync(OrderEntity entity, CancellationToken ct = default)
    {
        await dbContext.Orders.AddAsync(entity, ct);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<OrderEntity, Task> updateAction,
        CancellationToken ct = default)
    {
        var order = await dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id.Equals(id), ct)
            ?? throw new NotFoundException(nameof(id), id!);

        await updateAction(order);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<OrderEntity>, IQueryable<OrderEntity>> includes,
        Func<OrderEntity, Task> updateAction,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.Orders);
        var order = await query.FirstOrDefaultAsync(o => o.Id.Equals(id), ct)
            ?? throw new NotFoundException(nameof(id), id!);

        await updateAction(order);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var order = await dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order is not null)
            dbContext.Orders.Remove(order);
    }
}
