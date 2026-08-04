namespace SmartEcommerce.Order.Application.Features.Orders.DTOs;

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
