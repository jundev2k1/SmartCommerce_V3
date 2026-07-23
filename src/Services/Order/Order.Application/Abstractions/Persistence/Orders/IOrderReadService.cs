using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;

namespace Order.Application.Abstractions.Persistence.Orders;

public interface IOrderReadService
{
    Task<OrderEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PaginatedResult<OrderEntity>> SearchAsync(CriteriaRequest request, CancellationToken ct = default);

    /// <summary>Backs CreateOrderHandler's idempotency check - a prior order with the same (CustomerId, IdempotencyKey) is returned as-is instead of creating a duplicate.</summary>
    Task<OrderEntity?> GetByIdempotencyKeyAsync(Guid customerId, string idempotencyKey, CancellationToken ct = default);
}
