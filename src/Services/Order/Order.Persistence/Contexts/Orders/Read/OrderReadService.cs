using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Persistence.Ef.Criteria;

using Order.Application.Abstractions.Persistence.Orders;
using Order.Application.Features.Orders.Search;
using Order.Persistence.Engine;

namespace Order.Persistence.Contexts.Orders.Read;

public sealed class OrderReadService(OrderDbContext dbContext) : IOrderReadService
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

    public async Task<OrderEntity?> GetByIdempotencyKeyAsync(Guid customerId, string idempotencyKey, CancellationToken ct = default)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.IdempotencyKey == idempotencyKey, ct);
    }
}
