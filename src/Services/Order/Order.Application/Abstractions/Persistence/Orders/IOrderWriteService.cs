namespace Order.Application.Abstractions.Persistence.Orders;

public interface IOrderWriteService
{
    Task CreateAsync(OrderEntity order, CancellationToken ct = default);

    /// <summary>Updates the customer-editable shipping/contact snapshot (Order.UpdateOwnerInfo enforces the non-terminal-status guard).</summary>
    Task UpdateOwnerInfoAsync(
        Guid orderId,
        string ownerName,
        Email ownerEmail,
        PhoneNumber ownerPhone,
        string idempotencyKey,
        CancellationToken ct = default);

    Task<decimal> ConfirmAsync(Guid orderId, CancellationToken ct = default);

    Task<Guid> CancelAsync(Guid orderId, string reason, CancellationToken ct = default);

    Task<Guid> CompleteAsync(Guid orderId, CancellationToken ct = default);

    Task DeleteAsync(Guid orderId, CancellationToken ct = default);
}
