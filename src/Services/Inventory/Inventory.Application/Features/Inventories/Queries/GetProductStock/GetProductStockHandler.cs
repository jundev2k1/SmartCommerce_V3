using Inventory.Application.Abstractions.Repositories;

namespace Inventory.Application.Features.Inventories.Queries.GetProductStock;

public sealed class GetProductStockHandler(IInventoryRepository inventoryRepo)
    : IQueryHandler<GetProductStockQuery, GetProductStockResponse>
{
    public async Task<GetProductStockResponse> Handle(GetProductStockQuery request, CancellationToken ct = default)
    {
        var total = request.ProductVariationId is not null
            ? await inventoryRepo.GetTotalStockByVariationIdAsync(request.ProductVariationId.Value, ct)
            : await inventoryRepo.GetTotalStockByProductIdAsync(request.ProductId, ct);

        return new GetProductStockResponse(request.ProductId, request.ProductVariationId, total);
    }
}
