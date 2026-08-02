using Inventory.Application.Abstractions.Persistence.Inventories;
using Inventory.Application.Abstractions.Services;

namespace Inventory.Application.Services;

/// <summary>
/// Validates stock availability for multiple product variations in a single operation.
/// Centralizes validation logic and returns structured results for handlers to use.
/// </summary>
public sealed class StockAvailabilityService(IInventoryReadService readService) : IStockAvailabilityService
{
    public sealed record ValidationResult(
        bool Success,
        IReadOnlyList<InventoryStock> AvailableInventories,
        IReadOnlyList<InsufficientStockError> InsufficientItems);

    public sealed record InsufficientStockError(
        Guid InventoryId,
        Guid ProductVariationId,
        int RequestedQuantity,
        int AvailableQuantity);

    /// <summary>
    /// Validates that all requested product variations have sufficient stock in the specified warehouse.
    /// Returns structured result with available inventories and insufficient items.
    /// </summary>
    public async Task<ValidationResult> ValidateAsync(
        IReadOnlyList<(Guid ProductVariationId, int RequestedQuantity)> items,
        Guid warehouseId,
        CancellationToken ct = default)
    {
        if (items.Count == 0)
            return new ValidationResult(true, [], []);

        var variationIds = items.Select(i => i.ProductVariationId).Distinct().ToList();
        var inventories = await readService.GetByVariationAndWarehouseAsync(variationIds, warehouseId, ct);

        var available = new List<InventoryStock>();
        var insufficient = new List<InsufficientStockError>();

        foreach (var (variationId, requested) in items)
        {
            var inventory = inventories.FirstOrDefault(i => i.VariantId == variationId);

            if (inventory is null)
            {
                insufficient.Add(new InsufficientStockError(
                    id: Guid.Empty,
                    productVariationId: variationId,
                    requestedQuantity: requested,
                    availableQuantity: 0));
                continue;
            }

            if (inventory.AvailableQuantity < requested)
            {
                insufficient.Add(new InsufficientStockError(
                    id: inventory.Id,
                    productVariationId: variationId,
                    requestedQuantity: requested,
                    availableQuantity: inventory.AvailableQuantity));
                continue;
            }

            available.Add(inventory);
        }

        return new ValidationResult(
            Success: insufficient.Count == 0,
            AvailableInventories: available,
            InsufficientItems: insufficient);
    }
}

/// <summary>Extension method to fetch multiple inventories by variation/warehouse in single query.</summary>
file static class StockAvailabilityServiceExtensions
{
    private static async Task<IReadOnlyList<InventoryStock>> GetByVariationAndWarehouseAsync(
        this IInventoryReadService readService,
        IReadOnlyList<Guid> variationIds,
        Guid warehouseId,
        CancellationToken ct)
    {
        // For now, we fetch individually since batch variant/warehouse query doesn't exist yet.
        // TODO: Add GetByVariationAndWarehouseAsync(IReadOnlyList<Guid> variationIds, Guid warehouseId) to repository
        // This would fetch all in one query, improving performance.
        var results = new List<InventoryStock>(variationIds.Count);

        foreach (var variationId in variationIds)
        {
            var inventory = await readService.GetByVariationAndWarehouseAsync(variationId, warehouseId, ct);
            if (inventory is not null)
                results.Add(inventory);
        }

        return results;
    }
}
