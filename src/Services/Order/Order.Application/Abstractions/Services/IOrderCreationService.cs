using Order.Application.Features.Orders.Common;

namespace Order.Application.Abstractions.Services;

/// <summary>
/// The actual order-creation core (idempotency check, item validation/pricing/stock via
/// IOrderItemPreparationService, persistence, outbox event) - shared by the client-facing
/// CreateOrder flow (which additionally diffs against the caller's cart first) and the
/// admin-facing AdminCreateOrder flow (which skips that cart check entirely). CustomerId is
/// always resolved by the caller before reaching this service - never trusted from a request body
/// on the client path, always explicit on the admin path.
/// </summary>
public interface IOrderCreationService
{
    Task<CreateOrderResponse> CreateAsync(
        Guid customerId,
        string customerName,
        string customerPhone,
        string shippingAddress,
        IReadOnlyCollection<OrderItemRequest> items,
        CancellationToken ct = default);
}
