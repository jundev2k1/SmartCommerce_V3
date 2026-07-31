using BuildingBlock.Application.Exceptions;
using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.ValueObjects;

using Order.Application.Abstractions.Persistence.Orders;
using Order.Domain.Enums;
using Order.Domain.ValueObjects;
using Order.Persistence.Contexts.Orders.Repositories;
using Order.Persistence.Mapping.Orders;

namespace Order.Persistence.Contexts.Orders.Write;

public sealed class OrderWriteService(IOrderRepository orderRepo) : IOrderWriteService
{
    public async Task<OrderEntity> CreateAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        var order = OrderEntity.Create(OrderMapper.ToCreateOrderData(request));

        await orderRepo.AddAsync(order, ct);

        return order;
    }

    public async Task UpdateOwnerInfoAsync(
        Guid orderId,
        string ownerName,
        Email ownerEmail,
        PhoneNumber ownerPhone,
        string idempotencyKey,
        CancellationToken ct = default)
    {
        await orderRepo.UpdateAsync(
            id: orderId,
            updateAction: order =>
            {
                if (order.Status is not OrderStatus.Confirmed)
                    throw new BadRequestException(MessageCode.InvalidOrderStatus);

                order.UpdateOwnerInfo(ownerName, ownerEmail, ownerPhone, idempotencyKey);
            },
            ct);
    }

    public async Task<decimal> ConfirmAsync(Guid orderId, CancellationToken ct = default)
    {
        var totalAmount = 0m;

        await orderRepo.UpdateAsync(orderId, order =>
        {
            order.Accept();
            totalAmount = order.GrandTotal.Value;
        }, ct);

        return totalAmount;
    }

    public async Task<Guid> CancelAsync(Guid orderId, string reason, CancellationToken ct = default)
    {
        var customerId = Guid.Empty;

        await orderRepo.UpdateAsync(orderId, order =>
        {
            if (order.Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
                throw new BadRequestException(MessageCode.InvalidOrderStatus);

            order.Cancel(reason);
            customerId = order.Owner.OwnerId;
        }, ct);

        return customerId;
    }

    public async Task<Guid> CompleteAsync(Guid orderId, CancellationToken ct = default)
    {
        var customerId = Guid.Empty;

        await orderRepo.UpdateAsync(orderId, order =>
        {
            if (order.Status is not OrderStatus.Confirmed)
                throw new BadRequestException(MessageCode.InvalidOrderStatus);

            order.Complete();
            customerId = order.Owner.OwnerId;
        }, ct);

        return customerId;
    }

    public async Task DeleteAsync(Guid orderId, CancellationToken ct = default)
    {
        await orderRepo.DeleteByIdAsync(orderId, ct);
    }
}
