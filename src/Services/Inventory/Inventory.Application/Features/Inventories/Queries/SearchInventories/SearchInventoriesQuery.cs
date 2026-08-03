using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;

namespace SmartEcommerce.Inventory.Application.Features.Inventories.Queries.SearchInventories;

public sealed record SearchInventoriesQuery(CriteriaRequest Criteria) : IQuery<PaginatedResult<SearchInventoriesItemResponse>>;

public sealed record SearchInventoriesItemResponse(
    Guid Id,
    Guid ProductId,
    Guid ProductVariationId,
    Guid WarehouseId,
    int Quantity,
    DateTime CreatedAt,
    DateTime UpdatedAt);
