using SmartEcommerce.BuildingBlock.Domain.Metadata;

namespace SmartEcommerce.Order.Domain.Metadata;

public sealed class DiscountMetadata : MetadataBase
{
    public string? SourceName
    {
        get => Get<string>("source_name");
        set => Set(value, "source_name");
    }
}
