namespace NovaCore.Promotion.Domain.Entities.ProductSets;

/// <summary>A priced bundling arrangement of a ProductSet's items - BundlePrice/BundleRule/BundleGift reference it by BundleId only, no Navigation section was given for this entity either.</summary>
public sealed class ProductBundle : BaseEntity<Guid>, IAuditable
{
    public Guid ProductSetId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }

    private ProductBundle() { }

    /// <summary>Only ProductSet may construct a ProductBundle - see ProductSet.AddBundle.</summary>
    internal static ProductBundle Create(Guid productSetId, string name, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Bundle name cannot be empty.");

        return new ProductBundle
        {
            Id = Guid.CreateVersion7(),
            ProductSetId = productSetId,
            Name = name,
            DisplayOrder = displayOrder,
        };
    }
}
