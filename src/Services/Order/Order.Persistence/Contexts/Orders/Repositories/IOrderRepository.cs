namespace Order.Persistence.Contexts.Orders.Repositories;

public interface IOrderRepository
{
    Task<OrderEntity?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken ct = default);
}
