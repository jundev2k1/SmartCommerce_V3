using BuildingBlock.Application.Abstractions.Common;

using Order.Application.Abstractions.Repositories;

namespace Order.Application.Features.Orders.Queries.SearchOrders;

public sealed class SearchOrdersHandler(IOrderRepository orderRepo)
    : IQueryHandler<SearchOrdersQuery, PaginatedResult<SearchOrdersItemResponse>>
{
    public async Task<PaginatedResult<SearchOrdersItemResponse>> Handle(SearchOrdersQuery request, CancellationToken ct = default)
    {
        var result = await orderRepo.SearchAsync(request.Criteria, ct);
        var items = result.Items
            .Select(o => new SearchOrdersItemResponse(
                o.Id, o.CustomerId, o.CustomerName, o.CustomerPhone, o.Status, o.TotalAmount, o.CreatedAt, o.UpdatedAt))
            .ToList();

        return PaginatedResult<SearchOrdersItemResponse>.Create(items, result.PageNumber, result.PageSize, result.TotalCount);
    }
}
