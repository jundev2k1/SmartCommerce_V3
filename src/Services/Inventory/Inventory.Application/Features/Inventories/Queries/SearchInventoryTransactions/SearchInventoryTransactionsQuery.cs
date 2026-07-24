using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;

namespace Inventory.Application.Features.Inventories.Queries.SearchInventoryTransactions;

public sealed record SearchInventoryTransactionsQuery(CriteriaRequest Criteria) : IQuery<PaginatedResult<SearchInventoryTransactionsItemResponse>>;

public sealed record SearchInventoryTransactionsItemResponse(
    Guid Id,
    Guid InventoryId,
    Guid ProductId,
    Guid ProductVariationId,
    Guid WarehouseId,
    InventoryTransactionType Type,
    int Quantity,
    int QuantityAfter,
    string Reason,
    DateTime CreatedAt);
