namespace Order.Application.Abstractions.Persistence.Orders;

public interface IOrderWriteService
{
    Task CreateAsync(OrderEntity order, CancellationToken ct = default);

    /// <summary>Replaces the item list wholesale (Order.UpdateItems enforces the Pending-only guard). Returns the mutated totals so the caller can enqueue an outbox event without a second read.</summary>
    Task<(Guid CustomerId, decimal TotalAmount)> UpdateItemsAsync(
        Guid orderId,
        IReadOnlyCollection<OrderItemCreateModel> items,
        CancellationToken ct = default);

    Task<decimal> ConfirmAsync(Guid orderId, CancellationToken ct = default);

    Task<Guid> CancelAsync(Guid orderId, string reason, CancellationToken ct = default);

    Task<Guid> CompleteAsync(Guid orderId, CancellationToken ct = default);

    Task DeleteAsync(Guid orderId, CancellationToken ct = default);
}
