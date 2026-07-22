namespace Product.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Code,
    string Name,
    string Description,
    string Slug,
    IReadOnlyCollection<CreateProductVariationInput> Variations,
    IReadOnlyCollection<Guid>? CategoryIds = null,
    IReadOnlyCollection<Guid>? TagIds = null) : ICommand<CreateProductResponse>;

/// <summary>
/// One entry per initial variation. DisplayOrder is not accepted from the caller - the handler
/// assigns it from array position, since "initialize DisplayOrder correctly" is the server's
/// responsibility, not something a client-supplied value (which could have gaps/duplicates)
/// should drive.
/// </summary>
public sealed record CreateProductVariationInput(
    string Sku,
    decimal Price,
    bool IsDefault = false,
    string? Barcode = null,
    decimal? Cost = null,
    decimal? Weight = null,
    decimal? DimensionsLength = null,
    decimal? DimensionsWidth = null,
    decimal? DimensionsHeight = null,
    IReadOnlyCollection<string>? Images = null);

public sealed record CreateProductResponse(Guid ProductId, Guid DefaultVariationId);
