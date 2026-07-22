using Order.Application.Features.Orders.Common;

namespace Order.Application.Features.Orders.Commands.AdminCreateOrder;

/// <summary>Admin/Root-only counterpart to CreateOrderCommand - CustomerId is explicit since there's no "current user's cart" to source it (or diff against) for an order placed on someone else's behalf.</summary>
public sealed record AdminCreateOrderCommand(
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    string ShippingAddress,
    IReadOnlyCollection<OrderItemRequest> Items) : ICommand<CreateOrderResponse>;
