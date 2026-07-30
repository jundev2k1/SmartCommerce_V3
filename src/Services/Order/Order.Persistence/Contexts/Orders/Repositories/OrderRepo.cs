using Order.Persistence.Engine;

namespace Order.Persistence.Contexts.Orders.Repositories;

public sealed class OrderRepo(OrderDbContext dbContext)
    : OrderBaseRepository<OrderEntity>(dbContext), IOrderRepository
{
}
