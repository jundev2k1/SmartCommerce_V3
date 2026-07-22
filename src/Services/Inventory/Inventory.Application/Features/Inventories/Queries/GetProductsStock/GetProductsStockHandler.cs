using Inventory.Application.Abstractions.Repositories;

namespace Inventory.Application.Features.Inventories.Queries.GetProductsStock;

public sealed class GetProductsStockHandler(IInventoryRepository inventoryRepo)
    : IQueryHandler<GetProductsStockQuery, IReadOnlyCollection<ProductVariationStockResult>>
{
    public async Task<IReadOnlyCollection<ProductVariationStockResult>> Handle(GetProductsStockQuery request, CancellationToken ct = default)
    {
        var stockByVariation = await inventoryRepo.GetTotalStockByVariationIdsAsync(request.ProductVariationIds, ct);

        return [.. request.ProductVariationIds
            .Select(id => new ProductVariationStockResult(id, stockByVariation.GetValueOrDefault(id)))];
    }
}
