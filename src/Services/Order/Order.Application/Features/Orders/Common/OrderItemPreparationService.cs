using BuildingBlock.Application.Exceptions;

using Order.Application.Abstractions.Persistence.OrderProductCatalogs;
using Order.Application.Abstractions.Services;

namespace Order.Application.Features.Orders.Common;

public sealed class OrderItemPreparationService(
    IOrderProductCatalogReadService catalogReadService,
    IStockAvailabilityService stockAvailabilityService) : IOrderItemPreparationService
{
    /// <summary>Stock is checked first (cheapest gate, one batched gRPC call) so an insufficient-stock order fails fast before spending a query on catalog validation/pricing - applies uniformly to both CreateOrder (client) and AdminCreateOrder, since both funnel through here.</summary>
    public async Task<OrderItemCreateModel[]> PrepareAsync(IReadOnlyCollection<OrderItemRequest> items, CancellationToken ct = default)
    {
        await EnsureStockAvailableAsync(items, ct);
        return await ValidateAndPriceItemsAsync(items, ct);
    }

    /// <summary>
    /// The request's ProductId (semantically a ProductVariationId, see docs/services/order-service.md's
    /// naming note) is only trusted once it resolves against the locally synced OrderProductCatalog:
    /// unknown variation -> 404, disabled/discontinued/deleted variation -> rejected immediately.
    /// Name/price are always taken from the catalog snapshot, never from the request - only Discount
    /// is client-supplied, since there's no promotion/coupon module to derive it from.
    /// </summary>
    private async Task<OrderItemCreateModel[]> ValidateAndPriceItemsAsync(
        IReadOnlyCollection<OrderItemRequest> items, CancellationToken ct)
    {
        var variationIds = items.Select(i => i.VariationId).ToArray();
        var variations = await catalogReadService.GetByVariantionIdsAsync(variationIds, ct);

        var exceptVariations = variations.ExceptBy(variationIds, v => v.Id).ToArray();
        if (exceptVariations.Length > 0)
            throw new NotFoundException("Variation List", exceptVariations.Select(v => v.Id));

        var invalidProducts = variations
            .Where(v => !v.IsOrderable)
            .Select(v => v.Id.ToString())
            .ToArray();
        if (invalidProducts.Length > 0)
            throw ExceptionFactory.InvalidState(
                $"Product ({invalidProducts.JoinToString()}) is not currently available for ordering.");

        return [.. variations
            .Select(v =>
            {
                var request = items.First(i => i.VariationId == v.Id);
                return new OrderItemCreateModel(v.Id, v.ProductName, v.Price, request.Quantity, request.Discount);
            })];
    }

    /// <summary>Runs before catalog validation, so items are only known by VariationId here - not yet enriched with the catalog's ProductName.</summary>
    private async Task EnsureStockAvailableAsync(IReadOnlyCollection<OrderItemRequest> items, CancellationToken ct)
    {
        var requests = items.Select(i => new StockRequest(i.VariationId, i.Quantity)).ToArray();
        var availability = await stockAvailabilityService.CheckAsync(requests, ct);

        var insufficientItems = availability.Values.Where(a => !a.IsSufficient).ToArray();
        if (insufficientItems.Length == 0)
            return;

        var detail = new { insufficients = insufficientItems.Select(i => i.VariationId).ToArray() };
        var message = string.Join(", ", insufficientItems.Select(
            i => $"Variation {i.VariationId} (requested {i.RequestedQuantity}, available {i.AvailableQuantity})"));

        throw ExceptionFactory.InsufficientStock($"Insufficient stock for: {message}", detail);
    }
}
