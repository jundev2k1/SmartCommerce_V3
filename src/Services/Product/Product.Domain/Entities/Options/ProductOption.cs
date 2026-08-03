using SmartEcommerce.BuildingBlock.Domain.ValueObjects;
using ProductEntity = SmartEcommerce.Product.Domain.Entities.Products.Product;

namespace SmartEcommerce.Product.Domain.Entities.Options;

/// <summary>
/// Owned child of Product - a configurable dimension of the product (e.g. "Color", "Size").
/// When <see cref="IsVariantDimension"/> is true, its values combine with other dimensions to
/// produce Variants via <see cref="VariantOptionValue"/>; otherwise it is purely informational.
/// </summary>
public sealed class ProductOption : BaseEntity<Guid>
{
    public Guid ProductId { get; private set; }
    public ProductEntity Product { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsRequired { get; private set; }
    public bool IsVariantDimension { get; private set; }
    public CatalogStatus Status { get; private set; } = CatalogStatus.Active;

    public ICollection<ProductOptionValue> Values { get; private set; } = [];
    public ICollection<ProductOptionTranslation> Translations { get; private set; } = [];

    private ProductOption() { }

    public static ProductOption Create(
        Guid productId,
        string name,
        string description,
        string displayName,
        int displayOrder,
        bool isRequired = false,
        bool isVariantDimension = false,
        CatalogStatus status = CatalogStatus.Active)
    {
        ValidateName(name);

        return new ProductOption
        {
            Id = Guid.CreateVersion7(),
            ProductId = productId,
            Name = name,
            Description = description,
            DisplayName = displayName,
            DisplayOrder = displayOrder,
            IsRequired = isRequired,
            IsVariantDimension = isVariantDimension,
            Status = status,
        };
    }

    // ============================================================================
    // Option Values
    // Owns the selectable ProductOptionValue collection for this dimension:
    // creation, removal, and display-order sequencing. ProductOptionValue is
    // never constructed outside this entity.
    // ============================================================================

    #region Option Values

    public ProductOptionValue CreateOptionValue(
        string value,
        string? colorCode = null,
        Guid? imageId = null,
        CatalogStatus status = CatalogStatus.Active)
    {
        var displayOrder = Values.Count == 0 ? 0 : Values.Max(v => v.DisplayOrder) + 1;

        var optionValue = ProductOptionValue.Create(
            Id,
            value,
            displayOrder,
            colorCode,
            imageId,
            status);
        Values.Add(optionValue);

        return optionValue;
    }

    public void RemoveOptionValue(Guid valueId)
    {
        var value = Values.FirstOrDefault(v => v.Id == valueId);
        if (value is null)
            return;

        Values.Remove(value);
    }

    public void ReorderOptionValues(IEnumerable<Guid> orderedValueIds)
    {
        var idsInOrder = orderedValueIds.ToArray();

        for (var i = 0; i < idsInOrder.Length; i++)
        {
            var value = Values.FirstOrDefault(v => v.Id == idsInOrder[i])
                ?? throw ExceptionFactory.EntityNotFound<ProductOptionValue>(idsInOrder[i]);
            value.ChangeDisplayOrder(i);
        }
    }

    #endregion

    // ============================================================================
    // Translations
    // Manages the per-language display name/description override for this
    // option dimension, one entry per language code.
    // ============================================================================

    #region Translations

    public void Translate(
        LanguageCode languageCode,
        string displayName,
        string description)
    {
        ValidateName(displayName);

        var existingTranslation = Translations
            .FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existingTranslation != null)
        {
            existingTranslation.UpdateContent(displayName, description);
            return;
        }

        var translation = ProductOptionTranslation.Create(
            Id,
            languageCode,
            displayName,
            description);

        Translations.Add(translation);
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Core descriptive fields (name, description, display name, display order),
    // Required/VariantDimension/Active status transitions, and the shared
    // name-validation rule.
    // ============================================================================

    #region Details & lifecycle

    public void UpdateDetails(
        string name,
        string description,
        string displayName)
    {
        ValidateName(name);

        Name = name;
        Description = description;
        DisplayName = displayName;
    }

    public void ChangeDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    public void MarkAsRequired()
    {
        IsRequired = true;
    }

    public void MarkAsOptional()
    {
        IsRequired = false;
    }

    public void MarkAsVariantDimension()
    {
        IsVariantDimension = true;
    }

    public void UnmarkAsVariantDimension()
    {
        IsVariantDimension = false;
    }

    public void Activate()
    {
        Status = CatalogStatus.Active;
    }

    public void Deactivate()
    {
        Status = CatalogStatus.Inactive;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Option name cannot be empty.");
    }

    #endregion
}
