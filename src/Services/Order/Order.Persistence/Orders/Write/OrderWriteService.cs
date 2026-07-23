using BuildingBlock.Application.Exceptions;
using BuildingBlock.Domain.Enums;
using BuildingBlock.Persistence.Repository;

using Order.Application.Abstractions.Persistence.Orders;
using Order.Domain.Enums;

namespace Order.Persistence.Orders.Write;

/// <summary>
/// Every method here only stages repository operations - it never calls IUnitOfWork itself. The
/// Application handler (or saga step) owns the transaction, since every Order mutation historically
/// committed that way; EF Core automatically enlists this service's repo calls in whatever
/// transaction the caller opened.
/// </summary>
public sealed class OrderWriteService(IRepository<OrderEntity> repo) : IOrderWriteService
{
    public async Task CreateAsync(OrderEntity order, CancellationToken ct = default)
    {
        await repo.AddAsync(order, ct);
    }

    public async Task<(Guid CustomerId, decimal TotalAmount)> UpdateItemsAsync(
        Guid orderId,
        IReadOnlyCollection<OrderItemCreateModel> items,
        CancellationToken ct = default)
    {
        var customerId = Guid.Empty;
        var totalAmount = 0m;

        await repo.UpdateAsync(orderId, async order =>
        {
            order.UpdateItems(items);
            customerId = order.CustomerId;
            totalAmount = order.TotalAmount;
            await Task.CompletedTask;
        }, ct);

        return (customerId, totalAmount);
    }

    public async Task<decimal> ConfirmAsync(Guid orderId, CancellationToken ct = default)
    {
        var totalAmount = 0m;

        await repo.UpdateAsync(orderId, async order =>
        {
            order.Confirm();
            totalAmount = order.TotalAmount;
            await Task.CompletedTask;
        }, ct);

        return totalAmount;
    }

    public async Task<Guid> CancelAsync(Guid orderId, string reason, CancellationToken ct = default)
    {
        var customerId = Guid.Empty;

        await repo.UpdateAsync(orderId, async order =>
        {
            if (order.Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
                throw new BadRequestException(MessageCode.InvalidOrderStatus);

            order.Cancel(reason);
            customerId = order.CustomerId;
            await Task.CompletedTask;
        }, ct);

        return customerId;
    }

    public async Task<Guid> CompleteAsync(Guid orderId, CancellationToken ct = default)
    {
        var customerId = Guid.Empty;

        await repo.UpdateAsync(orderId, async order =>
        {
            if (order.Status is not OrderStatus.Confirmed)
                throw new BadRequestException(MessageCode.InvalidOrderStatus);

            order.Complete();
            customerId = order.CustomerId;
            await Task.CompletedTask;
        }, ct);

        return customerId;
    }

    public async Task DeleteAsync(Guid orderId, CancellationToken ct = default)
    {
        await repo.DeleteAsync(orderId, ct);
    }
}
