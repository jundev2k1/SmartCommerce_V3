using BuildingBlock.Criteria.Definition;

namespace Inventory.Application.Features.Inventories.Search;

/// <summary>Admin search whitelist for <see cref="InventoryTransaction"/> (stock movements). Built once (static singleton) - no per-request reflection scan.</summary>
public static class InventoryTransactionCriteriaDefinition
{
    public static readonly CriteriaDefinition<InventoryTransaction> Instance = CriteriaDefinition<InventoryTransaction>.Create()
        .Field(x => x.InventoryId).Guid()
        .Field(x => x.ProductId).Guid()
        .Field(x => x.VariationId).Guid()
        .Field(x => x.WarehouseId).Guid()
        .Field(x => x.Type).Enum().Sortable()
        .Field(x => x.Quantity).Number().Sortable()
        .Field(x => x.QuantityAfter).Number().Sortable()
        .Field(x => x.Reason).String().KeywordSearchable()
        .Field(x => x.CreatedAt).DateTime().Sortable()
        .Build();
}
