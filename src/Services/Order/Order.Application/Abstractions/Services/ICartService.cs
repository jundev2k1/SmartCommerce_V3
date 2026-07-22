namespace Order.Application.Abstractions.Services;

/// <summary>
/// Redis-backed per-user cart (see Order.Infrastructure.Caching.CartService). Every operation is
/// keyed by userId only - callers must resolve it from ICurrentUserService, never trust it from
/// a request body/query param.
/// </summary>
public interface ICartService
{
    Task<CartResponse> GetCartAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Adds Quantity to the existing line if the variation is already in the cart, otherwise inserts a new line.</summary>
    Task<CartResponse> AddItemAsync(Guid userId, Guid variationId, int quantity, CancellationToken ct = default);

    /// <summary>Sets the line to an absolute Quantity. Rejects Quantity &lt;= 0 - callers should remove the line instead.</summary>
    Task<CartResponse> UpdateItemQuantityAsync(Guid userId, Guid variationId, int quantity, CancellationToken ct = default);

    Task<CartResponse> RemoveItemAsync(Guid userId, Guid variationId, CancellationToken ct = default);

    Task ClearCartAsync(Guid userId, CancellationToken ct = default);
}

public sealed record CartItemResponse(Guid ProductId, Guid VariationId, string ProductName, decimal UnitPrice, int Quantity);

public sealed record CartResponse(IReadOnlyCollection<CartItemResponse> Items)
{
    public decimal TotalAmount => Items.Sum(i => i.UnitPrice * i.Quantity);
}
