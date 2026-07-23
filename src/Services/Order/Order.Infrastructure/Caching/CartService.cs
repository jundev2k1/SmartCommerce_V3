using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Domain.Exceptions;
using BuildingBlock.SharedKernel.Constants;

using Order.Application.Abstractions.Persistence.OrderProductCatalogs;
using Order.Application.Abstractions.Services;

namespace Order.Infrastructure.Caching;

/// <summary>
/// Redis-backed cart, keyed per user. Only {VariationId, Quantity} pairs are stored - name/price/
/// orderable-status are always resolved live from OrderProductCatalog on every read or mutation,
/// the same "never trust/store stale price" principle OrderItemPreparationService already applies
/// to order items (see docs/services/order-service.md). A variation that's been deleted or
/// deactivated since being added is dropped from the cart the next time it's read, rather than
/// silently kept.
/// </summary>
public sealed class CartService(
    ICacheService cacheService,
    IOrderProductCatalogReadService catalogReadService) : ICartService
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(CacheKeys.Cart.DefaultTtlMinutes);

    public async Task<CartResponse> GetCartAsync(Guid userId, CancellationToken ct = default)
    {
        var data = await LoadAsync(userId, ct);
        return await EnrichAndPruneAsync(userId, data, ct);
    }

    public async Task<CartResponse> AddItemAsync(Guid userId, Guid variationId, int quantity, CancellationToken ct = default)
    {
        await EnsureOrderableAsync(variationId, ct);

        var data = await LoadAsync(userId, ct);
        var existing = data.Items.FirstOrDefault(i => i.VariationId == variationId);
        if (existing is not null)
        {
            data.Items.Remove(existing);
            data.Items.Add(existing with { Quantity = existing.Quantity + quantity });
        }
        else
        {
            data.Items.Add(new CartLine(variationId, quantity));
        }

        await SaveAsync(userId, data, ct);
        return await EnrichAndPruneAsync(userId, data, ct);
    }

    public async Task<CartResponse> UpdateItemQuantityAsync(Guid userId, Guid variationId, int quantity, CancellationToken ct = default)
    {
        if (quantity <= 0)
            throw ExceptionFactory.InvalidRange("Quantity must be greater than 0 - remove the item instead of setting it to zero or less.");

        var data = await LoadAsync(userId, ct);
        var existing = data.Items.FirstOrDefault(i => i.VariationId == variationId)
            ?? throw new NotFoundException("Cart item", variationId);

        data.Items.Remove(existing);
        data.Items.Add(existing with { Quantity = quantity });

        await SaveAsync(userId, data, ct);
        return await EnrichAndPruneAsync(userId, data, ct);
    }

    public async Task<CartResponse> RemoveItemAsync(Guid userId, Guid variationId, CancellationToken ct = default)
    {
        var data = await LoadAsync(userId, ct);
        data.Items.RemoveAll(i => i.VariationId == variationId);

        await SaveAsync(userId, data, ct);
        return await EnrichAndPruneAsync(userId, data, ct);
    }

    public async Task ClearCartAsync(Guid userId, CancellationToken ct = default)
    {
        await cacheService.RemoveAsync(CacheKeys.Cart.Key(userId), ct);
    }

    private async Task EnsureOrderableAsync(Guid variationId, CancellationToken ct)
    {
        var variations = await catalogReadService.GetByVariantionIdsAsync([variationId], ct);
        var variation = variations.FirstOrDefault()
            ?? throw new NotFoundException("Variation", variationId);

        if (!variation.IsOrderable)
            throw ExceptionFactory.InvalidState($"Product ({variation.ProductName}) is not currently available for ordering.");
    }

    private async Task<CartData> LoadAsync(Guid userId, CancellationToken ct)
    {
        var data = await cacheService.GetAsync<CartData>(CacheKeys.Cart.Key(userId), ct);
        return data ?? new CartData([]);
    }

    private async Task SaveAsync(Guid userId, CartData data, CancellationToken ct)
    {
        await cacheService.SetAsync(CacheKeys.Cart.Key(userId), data, Ttl, ct);
    }

    /// <summary>Resolves live product/price data for every cart line, drops any line whose variation no longer exists or isn't orderable, and persists the pruned result.</summary>
    private async Task<CartResponse> EnrichAndPruneAsync(Guid userId, CartData data, CancellationToken ct)
    {
        if (data.Items.Count == 0)
            return new CartResponse([]);

        var variationIds = data.Items.Select(i => i.VariationId).ToArray();
        var catalogEntries = await catalogReadService.GetByVariantionIdsAsync(variationIds, ct);
        var catalogById = catalogEntries.ToDictionary(c => c.Id);

        var validItems = data.Items
            .Where(i => catalogById.TryGetValue(i.VariationId, out var entry) && entry.IsOrderable)
            .ToList();

        if (validItems.Count != data.Items.Count)
            await SaveAsync(userId, new CartData(validItems), ct);

        return new CartResponse([.. validItems
            .Select(i =>
            {
                var entry = catalogById[i.VariationId];
                return new CartItemResponse(entry.ProductId, i.VariationId, entry.ProductName, entry.Price, i.Quantity);
            })]);
    }
}

internal sealed record CartData(List<CartLine> Items);

internal sealed record CartLine(Guid VariationId, int Quantity);
