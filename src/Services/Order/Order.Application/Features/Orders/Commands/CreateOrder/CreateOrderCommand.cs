using Order.Application.Features.Orders.Common;

namespace Order.Application.Features.Orders.Commands.CreateOrder;

/// <summary>CustomerId is deliberately absent - the client path always derives identity from the caller's JWT claim (see CreateOrderHandler). AdminCreateOrderCommand is the on-behalf-of counterpart.</summary>
public sealed record CreateOrderCommand(
    string CustomerName,
    string CustomerPhone,
    string ShippingAddress,
    IReadOnlyCollection<OrderItemRequest> Items) : ICommand<CreateOrderResponse>;
