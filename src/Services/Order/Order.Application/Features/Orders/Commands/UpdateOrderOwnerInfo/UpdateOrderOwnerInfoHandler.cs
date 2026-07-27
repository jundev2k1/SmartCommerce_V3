using Order.Application.Abstractions.Persistence.Orders;

namespace Order.Application.Features.Orders.Commands.UpdateOrderOwnerInfo;

public sealed class UpdateOrderOwnerInfoHandler(
    IOrderWriteService orderWriteService,
    IUnitOfWork uow) : ICommandHandler<UpdateOrderOwnerInfoCommand, UpdateOrderOwnerInfoResponse>
{
    public async Task<UpdateOrderOwnerInfoResponse> Handle(UpdateOrderOwnerInfoCommand request, CancellationToken ct = default)
    {
        await uow.ExecuteTransactionAsync(async () =>
        {
            await orderWriteService.UpdateOwnerInfoAsync(
                request.OrderId,
                request.CustomerPhone,
                request.ShippingAddress,
                ct);
        }, ct: ct);

        return new UpdateOrderOwnerInfoResponse(
            request.OrderId,
            request.CustomerPhone,
            request.ShippingAddress);
    }
}
