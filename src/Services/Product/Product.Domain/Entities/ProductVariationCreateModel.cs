namespace Product.Domain.Entities;

/// <summary>
/// Element shape for the collection Product.Create accepts to initialize its required
/// variations in one call. This is NOT a general-purpose "Spec" object (see
/// ProductVariation.Create, which takes flat parameters) - it exists solely because a
/// *collection* of structured items needs a per-item shape; a flat-parameter method can't
/// express "N variations" the way it can express "one variation".
/// </summary>
public sealed record ProductVariationCreateModel(
    Sku Sku,
    decimal Price,
    bool IsDefault = false,
    Barcode? Barcode = null,
    decimal? Cost = null,
    decimal? Weight = null,
    Dimensions? Dimensions = null,
    IEnumerable<string>? Images = null,
    ProductVariationStatus Status = ProductVariationStatus.Active,
    ProductVariationMetadata? Metadata = null);
