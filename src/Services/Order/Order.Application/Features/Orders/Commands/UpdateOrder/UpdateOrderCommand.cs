using Order.Application.Features.Orders.Common;

namespace Order.Application.Features.Orders.Commands.UpdateOrder;

public sealed record UpdateOrderCommand(
    Guid OrderId,
    IReadOnlyCollection<OrderItemRequest> Items) : ICommand<UpdateOrderResponse>;

public sealed record UpdateOrderResponse(Guid OrderId, decimal TotalAmount, OrderStatus Status);
