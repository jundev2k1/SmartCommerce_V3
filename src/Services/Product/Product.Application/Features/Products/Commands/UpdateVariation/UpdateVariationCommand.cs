namespace Product.Application.Features.Products.Commands.UpdateVariation;

/// <summary>
/// Covers every ProductVariation attribute except DisplayOrder and IsDefault, which have their
/// own dedicated commands (ReorderVariations / SetDefaultVariation) since they carry
/// cross-variation invariants the aggregate root must mediate.
/// </summary>
public sealed record UpdateVariationCommand(
    Guid ProductId,
    Guid VariationId,
    string Sku,
    decimal Price,
    string Status,
    string? Barcode = null,
    decimal? Cost = null,
    decimal? Weight = null,
    decimal? DimensionsLength = null,
    decimal? DimensionsWidth = null,
    decimal? DimensionsHeight = null,
    IReadOnlyCollection<string>? Images = null) : ICommand<UpdateVariationResponse>;

public sealed record UpdateVariationResponse;
