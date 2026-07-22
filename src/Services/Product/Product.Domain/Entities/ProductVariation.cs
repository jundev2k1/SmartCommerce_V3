namespace Product.Domain.Entities;

public sealed class ProductVariation : BaseEntity<Guid>
{
    public Guid ProductId { get; private set; }
    public Sku Sku { get; private set; } = null!;
    public Barcode? Barcode { get; private set; }
    public decimal Price { get; private set; }
    public decimal? Cost { get; private set; }
    public decimal? Weight { get; private set; }
    public Dimensions? Dimensions { get; private set; }
    public ICollection<string> Images { get; private set; } = [];
    public ProductVariationStatus Status { get; private set; }
    public bool IsDefault { get; private set; }
    public int DisplayOrder { get; private set; }
    public ProductVariationMetadata Metadata { get; private set; } = new();

    private ProductVariation() { }

    /// <summary>
    /// Only Product may construct a variation - keeps the "exactly one Default" invariant from
    /// ever being bypassed by code outside the aggregate. Flat parameters (no Spec/DTO object)
    /// so call sites stay explicit and adding/removing a field never breaks every caller.
    /// </summary>
    internal static ProductVariation Create(
        Guid id,
        Guid productId,
        Sku sku,
        decimal price,
        int displayOrder,
        Barcode? barcode = null,
        decimal? cost = null,
        decimal? weight = null,
        Dimensions? dimensions = null,
        IEnumerable<string>? images = null,
        ProductVariationStatus status = ProductVariationStatus.Active,
        ProductVariationMetadata? metadata = null)
    {
        ValidatePrice(price);
        ValidateCost(cost);
        ValidateWeight(weight);

        var variation = new ProductVariation
        {
            Id = id,
            ProductId = productId,
            Sku = sku,
            Barcode = barcode,
            Price = price,
            Cost = cost,
            Weight = weight,
            Dimensions = dimensions,
            Status = status,
            DisplayOrder = displayOrder,
            Metadata = metadata ?? new ProductVariationMetadata(),
        };

        if (images is not null)
        {
            foreach (var image in images.Where(i => !string.IsNullOrWhiteSpace(i)).Distinct())
                variation.Images.Add(image);
        }

        return variation;
    }

    internal void MarkAsDefault()
    {
        IsDefault = true;
        Tourch();
    }

    internal void UnmarkAsDefault()
    {
        IsDefault = false;
        Tourch();
    }

    public void UpdatePricing(decimal price, decimal? cost)
    {
        ValidatePrice(price);
        ValidateCost(cost);

        Price = price;
        Cost = cost;
        Tourch();
    }

    public void UpdateIdentifiers(Sku sku, Barcode? barcode)
    {
        Sku = sku;
        Barcode = barcode;
        Tourch();
    }

    public void UpdatePhysicalAttributes(decimal? weight, Dimensions? dimensions)
    {
        ValidateWeight(weight);

        Weight = weight;
        Dimensions = dimensions;
        Tourch();
    }

    public void ChangeDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
        Tourch();
    }

    public void ReplaceImages(IEnumerable<string> urls)
    {
        Images.Clear();
        foreach (var url in urls.Where(url => !string.IsNullOrWhiteSpace(url)).Distinct())
            Images.Add(url);

        Tourch();
    }

    public void AddImage(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw ExceptionFactory.RequiredField("Image URL cannot be empty.");

        if (!Images.Contains(url))
            Images.Add(url);

        Tourch();
    }

    public void RemoveImage(string url)
    {
        Images.Remove(url);
        Tourch();
    }

    public void Activate()
    {
        Status = ProductVariationStatus.Active;
        Tourch();
    }

    public void Deactivate()
    {
        Status = ProductVariationStatus.Inactive;
        Tourch();
    }

    public void Discontinue()
    {
        Status = ProductVariationStatus.Discontinued;
        Tourch();
    }

    public void UpdateMetadata(ProductVariationMetadata metadata)
    {
        Metadata = metadata;
        Tourch();
    }

    /// <summary>Reusable outside Create/UpdatePricing so FluentValidation can check the exact same rule (see docs/context - Domain Coding Conventions).</summary>
    public static bool IsValidPrice(decimal price) => price >= 0;

    public static bool IsValidCost(decimal? cost) => cost is null || cost >= 0;

    public static bool IsValidWeight(decimal? weight) => weight is null || weight > 0;

    private static void ValidatePrice(decimal price)
    {
        if (!IsValidPrice(price))
            throw ExceptionFactory.InvalidRange("Variation price cannot be negative.");
    }

    private static void ValidateCost(decimal? cost)
    {
        if (!IsValidCost(cost))
            throw ExceptionFactory.InvalidRange("Variation cost cannot be negative.");
    }

    private static void ValidateWeight(decimal? weight)
    {
        if (!IsValidWeight(weight))
            throw ExceptionFactory.InvalidRange("Variation weight must be greater than zero when specified.");
    }
}
