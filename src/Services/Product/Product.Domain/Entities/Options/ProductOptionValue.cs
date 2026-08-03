using SmartEcommerce.BuildingBlock.SharedKernel.Extensions;

namespace SmartEcommerce.Product.Domain.Entities.Options;

/// <summary>
/// Owned child of ProductOption - one selectable value (e.g. "Red" for the "Color" option).
/// </summary>
public sealed class ProductOptionValue : BaseEntity<Guid>
{
    public Guid ProductOptionId { get; private set; }
    public ProductOption ProductOption { get; private set; } = default!;
    public string Value { get; private set; } = string.Empty;
    public string? ColorCode { get; private set; }
    public Guid? ImageId { get; private set; }
    public int DisplayOrder { get; private set; }
    public CatalogStatus Status { get; private set; } = CatalogStatus.Active;
    private ProductOptionValue() { }

    internal static ProductOptionValue Create(
        Guid productOptionId,
        string value,
        int displayOrder,
        string? colorCode = null,
        Guid? imageId = null,
        CatalogStatus status = CatalogStatus.Active)
    {
        ValidateValue(value);

        return new ProductOptionValue
        {
            Id = Guid.CreateVersion7(),
            ProductOptionId = productOptionId,
            Value = value,
            ColorCode = colorCode,
            ImageId = imageId,
            DisplayOrder = displayOrder,
            Status = status,
        };
    }

    public void UpdateValue(string value)
    {
        ValidateValue(value);

        Value = value;
    }

    public void UpdateAppearance(
        string? colorCode,
        Guid? imageId)
    {
        ColorCode = colorCode;
        ImageId = imageId;
    }

    public void ChangeDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    public void Activate()
    {
        Status = CatalogStatus.Active;
    }

    public void Deactivate()
    {
        Status = CatalogStatus.Inactive;
    }

    public static bool IsValidValue(string? value) => value.IsNotNullOrWhiteSpace();

    private static void ValidateValue(string value)
    {
        if (!IsValidValue(value))
            throw ExceptionFactory.RequiredField("Option value cannot be empty.");
    }
}
