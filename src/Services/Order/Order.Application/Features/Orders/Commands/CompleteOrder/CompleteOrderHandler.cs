using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Contract.Events.Order;
using BuildingBlock.Domain.Enums;

using Order.Application.Abstractions.Repositories;

namespace Order.Application.Features.Orders.Commands.CompleteOrder;

public sealed class CompleteOrderHandler(
    IOrderRepository orderRepo,
    IOutboxStore outboxStore,
    IUnitOfWork uow) : ICommandHandler<CompleteOrderCommand, CompleteOrderResponse>
{
    public async Task<CompleteOrderResponse> Handle(CompleteOrderCommand request, CancellationToken ct = default)
    {
        var customerId = Guid.Empty;

        await uow.ExecuteTransactionAsync(async () =>
        {
            await orderRepo.UpdateAsync(request.OrderId, async (order) =>
            {
                if (order.Status is not OrderStatus.Confirmed)
                    throw new BadRequestException(MessageCode.InvalidOrderStatus);

                order.Complete();
                customerId = order.CustomerId;
                await Task.CompletedTask;
            }, ct);

            var orderCompletedEvent = new OrderCompletedIntegrationEvent(request.OrderId, customerId);
            await outboxStore.EnqueueAsync(orderCompletedEvent, ct);
        }, ct: ct);

        return new CompleteOrderResponse(request.OrderId, OrderStatus.Completed);
    }
}
