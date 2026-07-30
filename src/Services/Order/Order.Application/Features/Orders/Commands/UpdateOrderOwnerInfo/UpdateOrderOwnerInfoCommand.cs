namespace Order.Application.Features.Orders.Commands.UpdateOrderOwnerInfo;

public sealed record UpdateOrderOwnerInfoCommand(
    Guid OrderId,
    string OwnerName,
    string OwnerEmail,
    string OwnerPhone) : ICommand;
