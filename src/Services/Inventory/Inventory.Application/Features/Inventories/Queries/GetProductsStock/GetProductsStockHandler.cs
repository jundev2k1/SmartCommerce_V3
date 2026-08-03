using SmartEcommerce.Inventory.Application.Abstractions.Persistence.Inventories;

namespace SmartEcommerce.Inventory.Application.Features.Inventories.Queries.GetProductsStock;

public sealed class GetProductsStockHandler(IInventoryReadService inventoryReadService)
    : IQueryHandler<GetProductsStockQuery, IReadOnlyCollection<VariantStockResult>>
{
    public async Task<IReadOnlyCollection<VariantStockResult>> Handle(GetProductsStockQuery request, CancellationToken ct = default)
    {
        var stockByVariation = await inventoryReadService.GetTotalStockByVariationIdsAsync(request.VariantIds, ct);

        return [.. request.VariantIds
            .Select(id => new VariantStockResult(id, stockByVariation.GetValueOrDefault(id)))];
    }
}
