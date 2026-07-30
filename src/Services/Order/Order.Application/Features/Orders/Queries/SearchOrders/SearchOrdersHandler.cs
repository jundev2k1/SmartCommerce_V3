using BuildingBlock.Application.Abstractions.Common;

using Order.Application.Abstractions.Persistence.Orders;

namespace Order.Application.Features.Orders.Queries.SearchOrders;

public sealed class SearchOrdersHandler(IOrderReadService orderReadService)
    : IQueryHandler<SearchOrdersQuery, PaginatedResult<SearchOrdersItemResponse>>
{
    public async Task<PaginatedResult<SearchOrdersItemResponse>> Handle(SearchOrdersQuery request, CancellationToken ct = default)
    {
        var result = await orderReadService.SearchAsync(request.Criteria, ct);
        var items = result.Items
            .Select(MapToResponse)
            .ToArray();

        return PaginatedResult<SearchOrdersItemResponse>.Create(
            items,
            result.PageNumber,
            result.PageSize,
            result.TotalCount);
    }

    private static SearchOrdersItemResponse MapToResponse(OrderEntity order)
    {
        return new SearchOrdersItemResponse(
            order.Id,
            order.Owner.OwnerId,
            order.Owner.OwnerName,
            order.Owner.OwnerPhone.Value,
            order.Status,
            order.GrandTotal.Value,
            order.CreatedAt,
            order.UpdatedAt);
    }
}
