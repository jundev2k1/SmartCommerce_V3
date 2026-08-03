using SmartEcommerce.BuildingBlock.Domain.ValueObjects;

namespace SmartEcommerce.Product.Domain.Entities.Options;

/// <summary>
/// Owned child of ProductOption - a locale-specific display name/description for the option
/// dimension. Composite-keyed by (ProductOptionId, LanguageCode): one entry per language.
/// </summary>
public sealed class ProductOptionTranslation : BaseEntity<Guid>
{
    public Guid ProductOptionId { get; private set; }
    public ProductOption ProductOption { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private ProductOptionTranslation() { }

    internal static ProductOptionTranslation Create(
        Guid productOptionId,
        LanguageCode languageCode,
        string name,
        string? description = null)
    {
        ValidateName(name);

        return new ProductOptionTranslation
        {
            Id = Guid.CreateVersion7(),
            ProductOptionId = productOptionId,
            LanguageCode = languageCode,
            Name = name,
            Description = description,
        };
    }

    public void UpdateContent(string name, string? description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Translated name cannot be empty.");
    }
}
