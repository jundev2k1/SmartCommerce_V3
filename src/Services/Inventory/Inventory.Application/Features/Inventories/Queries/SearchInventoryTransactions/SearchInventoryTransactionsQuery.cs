using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;

namespace SmartEcommerce.Inventory.Application.Features.Inventories.Queries.SearchInventoryTransactions;

public sealed record SearchInventoryTransactionsQuery(CriteriaRequest Criteria) : IQuery<PaginatedResult<SearchInventoryTransactionsItemResponse>>;

public sealed record SearchInventoryTransactionsItemResponse(
    Guid Id,
    Guid InventoryId,
    Guid ProductId,
    Guid VariantId,
    Guid WarehouseId,
    InventoryTransactionType Type,
    int Quantity,
    int QuantityAfter,
    string Reason,
    DateTime CreatedAt);
