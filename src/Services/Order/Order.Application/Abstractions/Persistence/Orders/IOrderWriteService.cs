using SmartEcommerce.Order.Application.Abstractions.Services;
using SmartEcommerce.Order.Application.Features.Orders.DTOs;

namespace SmartEcommerce.Order.Application.Abstractions.Persistence.Orders;

/// <summary>
/// Everything CreateOrderHandler needs Persistence to build an Order from - Application resolves
/// the caller's intent (cart/stock validation, catalog pricing via IOrderItemPreparationService)
/// but never constructs the Order/OrderOwner/OrderShipping/OrderItem entities itself. Persistence
/// maps this into SmartEcommerce.Order.Domain's CreateOrderData (see SmartEcommerce.Order.Persistence.Mapping.Orders.OrderMapper)
/// and lets Order.Create build itself.
/// </summary>
public sealed record CreateOrderRequest(
    string IdempotencyKey,
    Guid? CreatedById,
    OrderOwnerRequestDto Owner,
    OrderShippingInfoRequestDto ShippingInfo,
    IReadOnlyList<PreparedOrderItem> Items);

public interface IOrderWriteService
{
    /// <summary>
    /// Returns the created OrderEntity - CreateOrderHandler needs the whole aggregate (Id,
    /// OrderNumber, Items, GrandTotal) to build OrderCreatedIntegrationEvent, so returning a
    /// narrower DTO here would just be a redundant projection of the same data.
    /// </summary>
    Task<OrderEntity> CreateAsync(CreateOrderRequest request, CancellationToken ct = default);

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
