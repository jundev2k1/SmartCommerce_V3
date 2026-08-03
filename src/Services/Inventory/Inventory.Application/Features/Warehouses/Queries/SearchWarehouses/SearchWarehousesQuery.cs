using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;

namespace SmartEcommerce.Inventory.Application.Features.Warehouses.Queries.SearchWarehouses;

public sealed record SearchWarehousesQuery(CriteriaRequest Criteria) : IQuery<PaginatedResult<SearchWarehousesItemResponse>>;

public sealed record SearchWarehousesItemResponse(
    Guid Id,
    string Code,
    string Name,
    string Address,
    WarehouseStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
