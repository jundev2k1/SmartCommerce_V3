using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;

namespace SmartEcommerce.Inventory.Application.Abstractions.Services;

public interface IStockAvailabilityService : IService
{
    Task<StockAvailabilityResult> ValidateAsync(
        IReadOnlyList<(Guid ProductVariationId, int Quantity)> items,
        Guid warehouseId,
        CancellationToken ct = default);

    public sealed record StockAvailabilityResult(
        bool Success,
        IReadOnlyList<InventoryStock> AvailableInventories,
        IReadOnlyList<InsufficientStockError> InsufficientItems);

    public sealed record InsufficientStockError(
        Guid InventoryId,
        Guid ProductVariationId,
        int RequestedQuantity,
        int AvailableQuantity);
}
