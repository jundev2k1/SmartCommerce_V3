using NovaCore.BuildingBlock.Domain.Metadata;

namespace NovaCore.Promotion.Domain.Metadata;

public sealed class PromotionMetadata : MetadataBase
{
    public string? Note
    {
        get => Get<string>("note");
        set => Set(value, "note");
    }

    public string? ExternalReference
    {
        get => Get<string>("external_reference");
        set => Set(value, "external_reference");
    }
}
