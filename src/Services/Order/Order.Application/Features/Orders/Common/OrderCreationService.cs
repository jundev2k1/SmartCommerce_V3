using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Contract.Events.Order;

using Order.Application.Abstractions.Persistence.Orders;
using Order.Application.Abstractions.Services;

namespace Order.Application.Features.Orders.Common;

/// <summary>
/// PHASE 2/3/4 of the CreateOrder flow (see docs/reference/create-order-saga.md): validate the
/// request against the locally synced product catalog and check stock availability (both handled
/// by IOrderItemPreparationService), then persist a Pending order and an OrderCreatedIntegrationEvent
/// in one transaction. Never waits on inventory deduction/confirmation - that continues
/// asynchronously via CreateOrderSaga once the Outbox relays this event.
/// </summary>
public sealed class OrderCreationService(
    ICurrentUserService currentUser,
    IOrderReadService orderReadService,
    IOrderWriteService orderWriteService,
    IOrderItemPreparationService itemPreparation,
    IOutboxStore outboxStore,
    IUnitOfWork uow) : IOrderCreationService
{
    public async Task<CreateOrderResponse> CreateAsync(
        Guid customerId,
        string customerName,
        string customerPhone,
        string shippingAddress,
        IReadOnlyCollection<OrderItemRequest> items,
        CancellationToken ct = default)
    {
        var idempotencyKey = currentUser.GetCorrelationId()
            ?? throw new BadRequestException(MessageCode.InvalidInput, "Missing currelation ID from Header.");

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var duplicate = await orderReadService.GetByIdempotencyKeyAsync(customerId, idempotencyKey, ct);
            if (duplicate is not null)
                return new CreateOrderResponse(duplicate.Id, duplicate.TotalAmount, duplicate.Status);
        }

        var itemModels = await itemPreparation.PrepareAsync(items, ct);

        var order = OrderEntity.Create(
            Guid.CreateVersion7(),
            customerId,
            customerName,
            customerPhone,
            shippingAddress,
            itemModels,
            idempotencyKey);

        await uow.ExecuteTransactionAsync(async () =>
        {
            await orderWriteService.CreateAsync(order, ct);

            await outboxStore.EnqueueAsync(
                new OrderCreatedIntegrationEvent(
                    order.Id,
                    order.CustomerId,
                    [.. order.Items.Select(i => new OrderCreatedItem(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice))],
                    order.TotalAmount),
                ct);
        },
        ct: ct);

        return new CreateOrderResponse(order.Id, order.TotalAmount, order.Status);
    }
}
