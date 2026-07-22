namespace Product.Application.Features.Products.Commands.AddVariation;

public sealed record AddVariationCommand(
    Guid ProductId,
    string Sku,
    decimal Price,
    bool MakeDefault = false,
    string? Barcode = null,
    decimal? Cost = null,
    decimal? Weight = null,
    decimal? DimensionsLength = null,
    decimal? DimensionsWidth = null,
    decimal? DimensionsHeight = null,
    IReadOnlyCollection<string>? Images = null) : ICommand<AddVariationResponse>;

public sealed record AddVariationResponse(ProductVariationResponse Variation);
