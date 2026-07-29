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
            .Select(o => new SearchOrdersItemResponse(
                o.Id, o.Owner.OwnerId, o.Owner.OwnerName, o.Owner.OwnerPhone, o.Status, o.TotalAmount, o.CreatedAt, o.UpdatedAt))
            .ToList();

        return PaginatedResult<SearchOrdersItemResponse>.Create(items, result.PageNumber, result.PageSize, result.TotalCount);
    }
}
