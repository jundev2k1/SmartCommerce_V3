namespace Product.Application.Features.Products.DTOs;

public sealed record ProductVariationInputDto(
    string Sku,
    decimal Price,
    bool IsDefault = false,
    string? Barcode = null,
    decimal? Cost = null,
    decimal? Weight = null,
    decimal? DimensionsLength = null,
    decimal? DimensionsWidth = null,
    decimal? DimensionsHeight = null,
    IReadOnlyCollection<string>? Images = null
);
