using SmartEcommerce.Order.Persistence.Engine;

namespace SmartEcommerce.Order.Persistence.Contexts.Orders.Repositories;

public sealed class OrderRepo(OrderDbContext dbContext)
    : OrderBaseRepository<OrderEntity, Guid>(dbContext), IOrderRepository
{
    public async Task<OrderEntity?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken ct = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Owner)
            .FirstOrDefaultAsync(o => o.IdempotencyKey == idempotencyKey, ct);
    }
}
