namespace NovaCore.Promotion.Domain.Entities.ProductSets;

/// <summary>
/// Aggregate root for a ProductSet - owns Items/Bundles. BundlePrice/BundleRule/BundleGift are
/// related to a ProductBundle by BundleId only, not navigated from here or from ProductBundle
/// (no Navigation section given for ProductBundle itself) - see
/// docs/promotion-service/aggregates/product-set.md.
/// </summary>
public sealed class ProductSet : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public ProductSetCode Code { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProductSetStatus Status { get; private set; }
    public ProductSetType SetType { get; private set; }

    public ICollection<ProductSetItem> Items { get; private set; } = [];
    public ICollection<ProductBundle> Bundles { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private ProductSet() { }

    public static ProductSet Create(
        ProductSetCode code,
        string name,
        ProductSetType setType,
        string? description = null)
    {
        ValidateName(name);

        return new ProductSet
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            Description = description,
            Status = ProductSetStatus.Draft,
            SetType = setType,
        };
    }
    #endregion

    #region Details & lifecycle
    public void UpdateDetails(string name, string? description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Product set name cannot be empty.");
    }
    #endregion

    #region Status
    public void Activate()
    {
        if (Status != ProductSetStatus.Draft)
            throw ExceptionFactory.InvalidStatus($"Cannot activate a product set in {Status} status.");

        Status = ProductSetStatus.Active;
    }

    public void Archive()
    {
        if (Status == ProductSetStatus.Archived)
            throw ExceptionFactory.InvalidStatus("Product set is already archived.");

        Status = ProductSetStatus.Archived;
    }
    #endregion

    #region Item
    public void AddItem(Guid productId, Quantity quantity, Guid? variantId = null)
    {
        Items.Add(ProductSetItem.Create(Id, productId, quantity, variantId));
    }

    public void RemoveItem(Guid itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw ExceptionFactory.EntityNotFound<ProductSetItem>(itemId);

        Items.Remove(item);
    }
    #endregion

    #region Bundle
    public void AddBundle(string name, int displayOrder = 0)
    {
        Bundles.Add(ProductBundle.Create(Id, name, displayOrder));
    }

    public void RemoveBundle(Guid bundleId)
    {
        var bundle = Bundles.FirstOrDefault(b => b.Id == bundleId)
            ?? throw ExceptionFactory.EntityNotFound<ProductBundle>(bundleId);

        Bundles.Remove(bundle);
    }
    #endregion
}
