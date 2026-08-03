using SmartEcommerce.BuildingBlock.Domain.Metadata;

namespace SmartEcommerce.Product.Domain.Metadata;

/// <summary>Extensible, strongly-typed metadata specific to a single ProductVariation.</summary>
public sealed class ProductVariationMetadata : MetadataBase
{
    [Metadata("gift_wrap_eligible", DefaultValue = false)]
    public bool GiftWrapEligible
    {
        get => Get<bool>();
        set => Set(value);
    }

    [Metadata("warranty_months")]
    public int? WarrantyMonths
    {
        get => Get<int?>();
        set => Set(value);
    }

    [Metadata("care_instructions")]
    public string? CareInstructions
    {
        get => Get<string>();
        set => Set(value);
    }
}
