namespace Order.Application.Features.Orders.Commands.UpdateOrderOwnerInfo;

public sealed record UpdateOrderOwnerInfoCommand(
    Guid OrderId,
    string CustomerPhone,
    string ShippingAddress) : ICommand<UpdateOrderOwnerInfoResponse>;

public sealed record UpdateOrderOwnerInfoResponse(
    Guid OrderId,
    string CustomerPhone,
    string ShippingAddress);
