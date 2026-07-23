using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Contract.Events.Order;

using Order.Application.Abstractions.Persistence.Orders;
using Order.Application.Abstractions.Services;

namespace Order.Application.Features.Orders.Commands.UpdateOrder;

/// <summary>
/// Replaces a Pending order's item list wholesale - re-runs the same catalog/stock validation as
/// CreateOrder (via the shared IOrderItemPreparationService) since prices/availability may have
/// changed since the order was first created. Order.UpdateItems enforces the Pending-only guard.
/// </summary>
public sealed class UpdateOrderHandler(
    IOrderWriteService orderWriteService,
    IOrderItemPreparationService itemPreparation,
    IOutboxStore outboxStore,
    IUnitOfWork uow) : ICommandHandler<UpdateOrderCommand, UpdateOrderResponse>
{
    public async Task<UpdateOrderResponse> Handle(UpdateOrderCommand request, CancellationToken ct = default)
    {
        var itemModels = await itemPreparation.PrepareAsync(request.Items, ct);

        var totalAmount = 0m;

        await uow.ExecuteTransactionAsync(async () =>
        {
            var result = await orderWriteService.UpdateItemsAsync(request.OrderId, itemModels, ct);
            totalAmount = result.TotalAmount;

            await outboxStore.EnqueueAsync(
                new OrderUpdatedIntegrationEvent(request.OrderId, result.CustomerId, result.TotalAmount), ct);
        }, ct: ct);

        return new UpdateOrderResponse(request.OrderId, totalAmount, OrderStatus.Pending);
    }
}
