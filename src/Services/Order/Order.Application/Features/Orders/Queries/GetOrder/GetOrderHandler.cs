using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;

using Order.Application.Abstractions.Repositories;

namespace Order.Application.Features.Orders.Queries.GetOrder;

public sealed class GetOrderHandler(
    ICurrentUserService currentUser,
    IOrderRepository orderRepo) : IQueryHandler<GetOrderQuery, GetOrderResponse>
{
    public async Task<GetOrderResponse> Handle(GetOrderQuery request, CancellationToken ct = default)
    {
        var order = await orderRepo.GetByIdAsync(request.OrderId, ct)
            ?? throw new NotFoundException("Order", request.OrderId);

        // Endpoint only requires RequireAuthenticated (any logged-in user), not RequireAdmin, so
        // the owner-vs-admin distinction has to happen here: an admin dashboard needs to view any
        // order, but a regular customer may only view their own.
        var isAdmin = currentUser.IsInRole(AppRole.Admin) || currentUser.IsInRole(AppRole.Root);
        if (!isAdmin && order.CustomerId != currentUser.GetUserId())
            throw new ForbiddenException();

        return new GetOrderResponse(
            order.Id,
            order.CustomerId,
            order.CustomerName,
            order.CustomerPhone,
            order.ShippingAddress,
            order.Status,
            order.TotalAmount,
            [.. order.Items.Select(i => new GetOrderItemResponse(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.Discount, i.LineTotal))],
            order.CancellationReason,
            order.CreatedAt,
            order.UpdatedAt);
    }
}
