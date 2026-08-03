using SmartEcommerce.BuildingBlock.Domain.ValueObjects;

namespace SmartEcommerce.Product.Domain.Entities.Tags;

/// <summary>
/// Owned child of ProductTag - a locale-specific override of the tag's metadata.
/// </summary>
public sealed class ProductTagTranslation : BaseEntity<Guid>, IAuditable
{
    public ProductTag ProductTag { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private ProductTagTranslation() { }

    public static ProductTagTranslation Create(
        Guid id,
        LanguageCode languageCode,
        string name,
        string description)
    {
        ValidateName(name);

        return new ProductTagTranslation
        {
            Id = id,
            LanguageCode = languageCode,
            Name = name,
            Description = description,
        };
    }

    public void UpdateDetails(string name, string description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Tag name cannot be empty.");
    }
}
