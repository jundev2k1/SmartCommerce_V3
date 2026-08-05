using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using NovaCore.Order.Application.Abstractions.Persistence.Orders;
using NovaCore.Order.Application.Features.Orders.DTOs;
using NovaCore.Order.Domain.Enums;
using NovaCore.Order.Domain.ValueObjects;
using NovaCore.Order.Persistence.Contexts.Orders.Repositories;
using NovaCore.Order.Persistence.Mapping.Orders;

namespace NovaCore.Order.Persistence.Contexts.Orders.Write;

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

    public async Task<(Guid TenantId, decimal TotalAmount)> ConfirmAsync(Guid orderId, CancellationToken ct = default)
    {
        var tenantId = Guid.Empty;
        var totalAmount = 0m;

        await orderRepo.UpdateAsync(orderId, order =>
        {
            order.Accept();
            tenantId = order.TenantId;
            totalAmount = order.GrandTotal.Value;
        }, ct);

        return (tenantId, totalAmount);
    }

    public async Task<(Guid TenantId, Guid CustomerId)> CancelAsync(Guid orderId, string reason, CancellationToken ct = default)
    {
        var tenantId = Guid.Empty;
        var customerId = Guid.Empty;

        await orderRepo.UpdateAsync(orderId, order =>
        {
            if (order.Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
                throw new BadRequestException(MessageCode.InvalidOrderStatus);

            order.Cancel(reason);
            tenantId = order.TenantId;
            customerId = order.Owner.OwnerId;
        }, ct);

        return (tenantId, customerId);
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
