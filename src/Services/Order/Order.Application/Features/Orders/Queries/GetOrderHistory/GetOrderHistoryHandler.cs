using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;

using Order.Application.Abstractions.Persistence.Orders;

namespace Order.Application.Features.Orders.Queries.GetOrderHistory;

/// <summary>
/// Customer-facing order list: always scoped to the caller's own identity, unlike SearchOrders
/// (admin-only, any customer). No CustomerId is ever accepted from the request - see
/// IOrderReadService.SearchByCustomerAsync for where that scope is actually enforced.
/// </summary>
public sealed class GetOrderHistoryHandler(
    ICurrentUserService currentUser,
    IOrderReadService orderReadService) : IQueryHandler<GetOrderHistoryQuery, PaginatedResult<OrderHistoryItemResponse>>
{
    public async Task<PaginatedResult<OrderHistoryItemResponse>> Handle(GetOrderHistoryQuery request, CancellationToken ct = default)
    {
        var customerId = currentUser.GetUserId() ?? throw new ForbiddenException();

        var result = await orderReadService.SearchByCustomerAsync(customerId, request.Criteria, ct);
        var items = result.Items
            .Select(o => new OrderHistoryItemResponse(o.Id, o.Status, o.TotalAmount, o.CreatedAt, o.UpdatedAt))
            .ToList();

        return PaginatedResult<OrderHistoryItemResponse>.Create(items, result.PageNumber, result.PageSize, result.TotalCount);
    }
}
