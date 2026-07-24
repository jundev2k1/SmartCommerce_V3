using BuildingBlock.Criteria.Definition;

namespace Inventory.Application.Features.Inventories.Search;

/// <summary>Admin search whitelist for <see cref="InventoryEntity"/>. Built once (static singleton) - no per-request reflection scan. No keyword-searchable field - stock rows have no free-text content.</summary>
public static class InventoryCriteriaDefinition
{
    public static readonly CriteriaDefinition<InventoryEntity> Instance = CriteriaDefinition<InventoryEntity>.Create()
        .Field(x => x.ProductId).Guid()
        .Field(x => x.ProductVariationId).Guid()
        .Field(x => x.WarehouseId).Guid()
        .Field(x => x.Quantity).Number().Sortable()
        .Field(x => x.CreatedAt).DateTime().Sortable()
        .Build();
}
