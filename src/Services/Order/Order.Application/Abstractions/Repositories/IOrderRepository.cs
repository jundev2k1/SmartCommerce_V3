using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;

namespace Order.Application.Abstractions.Repositories;

public interface IOrderRepository
{
    Task<OrderEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PaginatedResult<OrderEntity>> SearchAsync(CriteriaRequest request, CancellationToken ct = default);

    /// <summary>Backs CreateOrderHandler's idempotency check - a prior order with the same (CustomerId, IdempotencyKey) is returned as-is instead of creating a duplicate.</summary>
    Task<OrderEntity?> GetByIdempotencyKeyAsync(Guid customerId, string idempotencyKey, CancellationToken ct = default);

    Task<OrderEntity?> GetByIdAsync(
        Guid id,
        Func<IQueryable<OrderEntity>, IQueryable<OrderEntity>> includes,
        CancellationToken ct = default);

    Task AddAsync(OrderEntity entity, CancellationToken ct = default);

    Task UpdateAsync<TId>(
        TId id,
        Func<OrderEntity, Task> updateAction,
        CancellationToken ct = default);

    Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<OrderEntity>, IQueryable<OrderEntity>> includes,
        Func<OrderEntity, Task> updateAction,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
