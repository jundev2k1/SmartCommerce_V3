using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Criteria;

using SmartEcommerce.Order.Application.Abstractions.Persistence.Orders;
using SmartEcommerce.Order.Application.Features.Orders.Search;
using SmartEcommerce.Order.Persistence.Contexts.Orders.Repositories;
using SmartEcommerce.Order.Persistence.Engine;

namespace SmartEcommerce.Order.Persistence.Contexts.Orders.Read;

public sealed class OrderReadService(
    IOrderRepository orderRepo,
    OrderDbContext dbContext) : IOrderReadService
{
    public async Task<OrderEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await orderRepo.GetByIdAsync(
            id,
            query => query
                .Include(q => q.Items)
                .Include(q => q.Owner),
            ct);
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
            .Where(o => o.Owner.OwnerId == customerId)
            .ApplyCriteria(OrderHistoryCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }

    public async Task<OrderEntity?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken ct = default)
        => await orderRepo.GetByIdempotencyKeyAsync(idempotencyKey, ct);
}
