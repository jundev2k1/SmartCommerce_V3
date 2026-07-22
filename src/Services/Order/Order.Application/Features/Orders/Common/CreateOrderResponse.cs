namespace Order.Application.Features.Orders.Common;

public sealed record CreateOrderResponse(Guid OrderId, decimal TotalAmount, OrderStatus Status);
