using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Contract.Events.Order;
using BuildingBlock.Domain.Enums;

using Order.Application.Abstractions.Repositories;
using Order.Application.Abstractions.Services;

namespace Order.Application.Features.Orders.Commands.CancelOrder;

public sealed class CancelOrderHandler(
    IOrderRepository orderRepo,
    IOutboxStore outboxStore,
    IInventoryClientService inventoryClient,
    IUnitOfWork uow) : ICommandHandler<CancelOrderCommand, CancelOrderResponse>
{
    public async Task<CancelOrderResponse> Handle(CancelOrderCommand request, CancellationToken ct = default)
    {
        var customerId = Guid.Empty;

        await uow.ExecuteTransactionAsync(async () =>
        {
            await orderRepo.UpdateAsync(request.OrderId, async (order) =>
            {
                if (order.Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
                    throw new BadRequestException(MessageCode.InvalidOrderStatus);

                order.Cancel(request.Reason);
                customerId = order.CustomerId;
                await Task.CompletedTask;
            }, ct);

            var orderCancelledEvent = new OrderCancelledIntegrationEvent(
                request.OrderId,
                customerId,
                request.Reason);
            await outboxStore.EnqueueAsync(orderCancelledEvent, ct);
        }, ct: ct);

        // Deduction is keyed by OrderId and idempotent (StockDeduction ledger in Inventory), so
        // this is safe to call unconditionally even if the order was still Pending and
        // DeductInventoryStep hadn't run yet - it's a no-op when there's nothing to reverse.
        await inventoryClient.RestockAsync(request.OrderId, reason: request.Reason ?? "Order cancelled", ct);

        return new CancelOrderResponse(request.OrderId, OrderStatus.Cancelled);
    }
}
