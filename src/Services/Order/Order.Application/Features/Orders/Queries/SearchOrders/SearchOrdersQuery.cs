using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;

namespace SmartEcommerce.Order.Application.Features.Orders.Queries.SearchOrders;

public sealed record SearchOrdersQuery(CriteriaRequest Criteria) : IQuery<PaginatedResult<SearchOrdersItemResponse>>;

public sealed record SearchOrdersItemResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
