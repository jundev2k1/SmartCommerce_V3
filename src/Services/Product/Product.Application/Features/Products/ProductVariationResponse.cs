namespace Product.Application.Features.Products;

/// <summary>Shared response shape for a ProductVariation, reused across every Products query/command that returns one.</summary>
public sealed record ProductVariationResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Barcode,
    decimal Price,
    decimal? Cost,
    decimal? Weight,
    decimal? DimensionsLength,
    decimal? DimensionsWidth,
    decimal? DimensionsHeight,
    IReadOnlyCollection<string> Images,
    string Status,
    bool IsDefault,
    int DisplayOrder,
    // Null when the caller didn't resolve stock for this variation (e.g. AddVariation/UpdateVariation
    // responses, which don't call Inventory) or Inventory Service couldn't be reached - not the same
    // as 0 (confirmed out of stock). Only GetProduct currently populates this (see docs/tasks/2026-07-27/
    // Task22_get-product-no-per-variation-stock.md).
    int? AvailableStock = null)
{
    public static ProductVariationResponse From(ProductVariation variation, int? availableStock = null) => new(
        variation.Id,
        variation.Sku.Value,
        variation.Name,
        variation.Barcode?.Value,
        variation.Price,
        variation.Cost,
        variation.Weight,
        variation.Dimensions?.Length,
        variation.Dimensions?.Width,
        variation.Dimensions?.Height,
        [.. variation.Images],
        variation.Status.ToString(),
        variation.IsDefault,
        variation.DisplayOrder,
        availableStock);
}
