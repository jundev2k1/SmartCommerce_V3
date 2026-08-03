namespace SmartEcommerce.Product.Domain.Entities.Products;

/// <summary>
/// Aggregate root. Holds only data shared across every variation - identity, display copy,
/// category/tag membership, and shared metadata. Everything variation-specific (Sku, price,
/// stock-affecting attributes, status, ...) lives on <see cref="ProductVariation"/>.
/// </summary>
public sealed class Product : AggregateRoot<Guid>, IAuditable
{
    public ProductCode Code { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Slug Slug { get; private set; } = null!;
    public ProductMetadata Metadata { get; private set; } = new();

    public ICollection<ProductVariation> Variations { get; private set; } = [];
    public ICollection<ProductCategoryMapping> CategoryMappings { get; private set; } = [];
    public ICollection<ProductTagMapping> TagMappings { get; private set; } = [];

    public ProductVariation DefaultVariation => Variations.First(v => v.IsDefault);

    private Product() { }

    /// <summary>
    /// Creates a Product together with its required variations in one call - aggregate
    /// initialization is naturally a bulk operation, so the caller just supplies the collection
    /// and every invariant (at least one variation, exactly one Default, DisplayOrder) is
    /// enforced and initialized here rather than split across the caller.
    /// </summary>
    public static Product Create(
        ProductCode code,
        string name,
        string description,
        Slug slug,
        ProductMetadata? metadata,
        IEnumerable<ProductVariation> variations,
        IEnumerable<Guid> categoryIds,
        IEnumerable<Guid> tagIds)
    {
        ValidateName(name);

        if (!variations.Any())
            throw ExceptionFactory.EmptyCollection("A product must be created with at least one variation.");

        if (categoryIds.Distinct().Count() != categoryIds.Count())
            throw ExceptionFactory.Duplicate($"One or more categories are dupplicated.");
        if (tagIds.Distinct().Count() != tagIds.Count())
            throw ExceptionFactory.Duplicate($"One or more tags are dupplicated.");

        var categoryMappings = categoryIds
            .Select(id => ProductCategoryMapping.Create(default, id))
            .ToArray();
        var tagMappings = tagIds
            .Select(id => ProductTagMapping.Create(default, id))
            .ToArray();
        var product = new Product
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            Description = description,
            Slug = slug,
            Metadata = metadata ?? new ProductMetadata(),
            Variations = [.. variations],
            CategoryMappings = categoryMappings,
            TagMappings = tagMappings,
        };

        return product;
    }

    /// <summary>
    /// Removes a variation. The last remaining variation can never be removed. Removing the
    /// current Default auto-promotes the remaining variation with the lowest DisplayOrder, so
    /// "exactly one Default" holds at every method boundary without an extra caller round-trip.
    /// </summary>
    public void RemoveVariation(Guid variationId)
    {
        if (Variations.Count <= 1)
            throw ExceptionFactory.InvalidState("Cannot remove the last variation of a product.");

        var variation = Variations.FirstOrDefault(v => v.Id == variationId)
            ?? throw ExceptionFactory.EntityNotFound<ProductVariation>(variationId);

        var wasDefault = variation.IsDefault;
        Variations.Remove(variation);

        if (wasDefault)
        {
            var successor = Variations.OrderBy(v => v.DisplayOrder).First();
            successor.MarkAsDefault();
        }
    }

    /// <summary>Switches the Default variation. No-op if it already is the default.</summary>
    public void SetDefaultVariation(Guid variationId)
    {
        var target = Variations.FirstOrDefault(v => v.Id == variationId)
            ?? throw ExceptionFactory.EntityNotFound<ProductVariation>(variationId);

        if (target.IsDefault)
            return;

        foreach (var variation in Variations.Where(v => v.IsDefault))
            variation.UnmarkAsDefault();

        target.MarkAsDefault();
    }

    public void AssignCategory(Guid categoryId)
    {
        if (CategoryMappings.Any(m => m.CategoryId == categoryId))
            return;

        CategoryMappings.Add(
            ProductCategoryMapping.Create(Id, categoryId));
    }

    public void RemoveCategory(Guid categoryId)
    {
        var mapping = CategoryMappings.FirstOrDefault(m => m.CategoryId == categoryId);
        if (mapping is null)
            return;

        CategoryMappings.Remove(mapping);
    }

    public void AssignTag(Guid tagId)
    {
        if (TagMappings.Any(m => m.TagId == tagId))
            return;

        TagMappings.Add(
            ProductTagMapping.Create(Id, tagId));
    }

    public void RemoveTag(Guid tagId)
    {
        var mapping = TagMappings.FirstOrDefault(m => m.TagId == tagId);
        if (mapping is null)
            return;

        TagMappings.Remove(mapping);
    }

    public void UpdateDetails(string name, string description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public void ChangeSlug(Slug slug)
    {
        Slug = slug;
    }

    public void UpdateMetadata(ProductMetadata metadata)
    {
        Metadata = metadata;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Product name cannot be empty.");
    }
}
