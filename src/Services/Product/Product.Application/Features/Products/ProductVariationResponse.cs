namespace Product.Application.Features.Products;

/// <summary>Shared response shape for a ProductVariation, reused across every Products query/command that returns one.</summary>
public sealed record ProductVariationResponse(
    Guid Id,
    string Sku,
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
    int DisplayOrder)
{
    public static ProductVariationResponse From(ProductVariation variation) => new(
        variation.Id,
        variation.Sku.Value,
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
        variation.DisplayOrder);
}
