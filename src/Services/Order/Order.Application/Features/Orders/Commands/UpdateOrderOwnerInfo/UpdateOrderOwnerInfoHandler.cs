using Order.Application.Abstractions.Persistence.Orders;
using Order.Domain.ValueObjects;

namespace Order.Application.Features.Orders.Commands.UpdateOrderOwnerInfo;

public sealed class UpdateOrderOwnerInfoHandler(
    IOrderWriteService orderWriteService,
    IUnitOfWork uow) : ICommandHandler<UpdateOrderOwnerInfoCommand>
{
    public async Task Handle(UpdateOrderOwnerInfoCommand request, CancellationToken ct = default)
    {
        await uow.ExecuteTransactionAsync(async () =>
        {
            await orderWriteService.UpdateOwnerInfoAsync(
                request.OrderId,
                request.OwnerName,
                Email.Create(request.OwnerEmail),
                PhoneNumber.Create(request.OwnerPhone),
                ct);
        }, ct: ct);
    }
}
