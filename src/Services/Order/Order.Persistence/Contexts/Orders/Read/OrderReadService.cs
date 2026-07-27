using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Persistence.Ef.Criteria;

using Order.Application.Abstractions.Persistence.Orders;
using Order.Application.Features.Orders.Search;
using Order.Persistence.Engine;

namespace Order.Persistence.Contexts.Orders.Read;

public sealed class OrderReadService(OrderDbContext dbContext) : IOrderReadService
{
    // Items/Owner are now real (non-owned) relationships, so unlike before they need an explicit
    // Include - every call site below previously got both for free. Including them here keeps
    // that same "always loaded" behavior (TotalAmount, customer/shipping snapshot, ...) for every
    // caller.
    public async Task<OrderEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Owner)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<PaginatedResult<OrderEntity>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Owner)
            .ApplyCriteria(OrderCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }

    public async Task<PaginatedResult<OrderEntity>> SearchByCustomerAsync(Guid customerId, CriteriaRequest request, CancellationToken ct = default)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Owner)
            .Where(o => o.Owner.CustomerId == customerId)
            .ApplyCriteria(OrderHistoryCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }

    public async Task<OrderEntity?> GetByIdempotencyKeyAsync(Guid customerId, string idempotencyKey, CancellationToken ct = default)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Owner)
            .FirstOrDefaultAsync(o => o.Owner.CustomerId == customerId && o.Owner.IdempotencyKey == idempotencyKey, ct);
    }
}
