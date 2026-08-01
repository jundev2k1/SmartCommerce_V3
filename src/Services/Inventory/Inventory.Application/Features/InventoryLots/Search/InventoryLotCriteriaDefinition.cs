using BuildingBlock.Criteria.Definition;

namespace Inventory.Application.Features.InventoryLots.Search;

public static class InventoryLotCriteriaDefinition
{
    public static readonly CriteriaDefinition<InventoryLot> Instance = CriteriaDefinition<InventoryLot>.Create()
        .Field(x => x.InventoryId).Guid()
        .Field(x => x.LotNumber).Text().Sortable()
        .Field(x => x.Status).Enum().Sortable()
        .Field(x => x.ManufactureDate).DateTime().Sortable()
        .Field(x => x.ExpiredDate).DateTime().Sortable()
        .Field(x => x.CreatedAt).DateTime().Sortable()
        .Build();
}
