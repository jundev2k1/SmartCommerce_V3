namespace Product.Application.Features.Products;

/// <summary>Shared response shape for a ProductVariation, reused across every Products query/command that returns one.</summary>
public sealed record ProductVariationResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Barcode,
    decimal Price,
    decimal? Weight,
    string? WeightUnit,
    decimal? DimensionsLength,
    decimal? DimensionsWidth,
    decimal? DimensionsHeight,
    IReadOnlyCollection<string> Images,
    string Status,
    bool IsDefault,
    int DisplayOrder,
    int? AvailableStock = null)
{
    public static ProductVariationResponse From(ProductVariation variation, int? availableStock = null) => new(
        variation.Id,
        variation.Sku.Value,
        variation.Name,
        variation.Barcode?.Value,
        variation.Price.Value,
        variation.Weight?.Value,
        variation.Weight?.Unit.ToString(),
        variation.Dimensions?.Length,
        variation.Dimensions?.Width,
        variation.Dimensions?.Height,
        [.. variation.Images],
        variation.Status.ToString(),
        variation.IsDefault,
        variation.DisplayOrder,
        availableStock);
}
