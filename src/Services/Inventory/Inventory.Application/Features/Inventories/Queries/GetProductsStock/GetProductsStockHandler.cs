using SmartEcommerce.Inventory.Application.Abstractions.Persistence.Inventories;

namespace SmartEcommerce.Inventory.Application.Features.Inventories.Queries.GetProductsStock;

public sealed class GetProductsStockHandler(IInventoryReadService inventoryReadService)
    : IQueryHandler<GetProductsStockQuery, IReadOnlyCollection<ProductVariationStockResult>>
{
    public async Task<IReadOnlyCollection<ProductVariationStockResult>> Handle(GetProductsStockQuery request, CancellationToken ct = default)
    {
        var stockByVariation = await inventoryReadService.GetTotalStockByVariationIdsAsync(request.ProductVariationIds, ct);

        return [.. request.ProductVariationIds
            .Select(id => new ProductVariationStockResult(id, stockByVariation.GetValueOrDefault(id)))];
    }
}
