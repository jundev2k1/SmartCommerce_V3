namespace SmartEcommerce.Product.Domain.Entities.Products;

public sealed class Variant : BaseEntity<Guid>
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Sku Sku { get; private set; } = null!;
    public Barcode? Barcode { get; private set; }
    public Money Price { get; private set; } = default!;
    public Weight? Weight { get; private set; }
    public Dimensions? Dimensions { get; private set; }
    public ICollection<string> Images { get; private set; } = [];
    public VariantStatus Status { get; private set; }
    public bool IsDefault { get; private set; }
    public int DisplayOrder { get; private set; }
    public VariantMetadata Metadata { get; private set; } = new();

    private Variant() { }

    public static Variant Create(
        Guid productId,
        Sku sku,
        string name,
        Money price,
        int displayOrder,
        Barcode? barcode = null,
        Weight? weight = null,
        Dimensions? dimensions = null,
        IEnumerable<string>? images = null,
        VariantStatus status = VariantStatus.Active,
        VariantMetadata? metadata = null)
    {
        ValidateName(name);

        var variation = new Variant
        {
            Id = Guid.CreateVersion7(),
            ProductId = productId,
            Sku = sku,
            Name = name,
            Barcode = barcode,
            Price = price,
            Weight = weight,
            Dimensions = dimensions,
            Status = status,
            DisplayOrder = displayOrder,
            Metadata = metadata ?? new VariantMetadata(),
        };

        if (images is not null)
        {
            foreach (var image in images.Where(i => !string.IsNullOrWhiteSpace(i)).Distinct())
                variation.Images.Add(image);
        }

        return variation;
    }

    public void UpdateName(string name)
    {
        ValidateName(name);

        Name = name;
    }

    public void MarkAsDefault()
    {
        IsDefault = true;
    }

    public void UnmarkAsDefault()
    {
        IsDefault = false;
    }

    public void UpdatePricing(Money price)
    {
        Price = price;
    }

    public void UpdateIdentifiers(Sku sku, Barcode? barcode)
    {
        Sku = sku;
        Barcode = barcode;
    }

    public void UpdatePhysicalAttributes(Weight? weight, Dimensions? dimensions)
    {
        Weight = weight;
        Dimensions = dimensions;
    }

    public void ChangeDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    public void ReplaceImages(IEnumerable<string> urls)
    {
        Images.Clear();
        foreach (var url in urls.Where(url => !string.IsNullOrWhiteSpace(url)).Distinct())
            Images.Add(url);
    }

    public void AddImage(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw ExceptionFactory.RequiredField("Image URL cannot be empty.");

        if (!Images.Contains(url))
            Images.Add(url);
    }

    public void RemoveImage(string url)
    {
        Images.Remove(url);
    }

    public void Activate()
    {
        Status = VariantStatus.Active;
    }

    public void Deactivate()
    {
        Status = VariantStatus.Inactive;
    }

    public void Discontinue()
    {
        Status = VariantStatus.Discontinued;
    }

    public void UpdateMetadata(VariantMetadata metadata)
    {
        Metadata = metadata;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Variation name cannot be empty.");
    }
}
