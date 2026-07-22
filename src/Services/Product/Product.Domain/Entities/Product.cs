namespace Product.Domain.Entities;

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
        Guid id,
        ProductCode code,
        string name,
        string description,
        Slug slug,
        IEnumerable<ProductVariationCreateModel> variations,
        ProductMetadata? metadata = null)
    {
        ValidateName(name);

        var models = variations.ToList();
        if (models.Count == 0)
            throw ExceptionFactory.EmptyCollection("A product must be created with at least one variation.");

        var product = new Product
        {
            Id = id,
            Code = code,
            Name = name,
            Description = description,
            Slug = slug,
            Metadata = metadata ?? new ProductMetadata(),
        };

        for (var i = 0; i < models.Count; i++)
        {
            var model = models[i];
            var variation = ProductVariation.Create(
                Guid.CreateVersion7(),
                id,
                model.Sku,
                model.Price,
                i,
                model.Barcode,
                model.Cost,
                model.Weight,
                model.Dimensions,
                model.Images,
                model.Status,
                model.Metadata);

            product.Variations.Add(variation);
        }

        var defaultIndex = models.FindIndex(m => m.IsDefault);
        var defaultVariation = defaultIndex >= 0 ? product.Variations.ElementAt(defaultIndex) : product.Variations.First();
        defaultVariation.MarkAsDefault();

        return product;
    }

    /// <summary>Adds a new variation. Flat parameters (no Spec/DTO object) - see ProductVariation.Create remarks.</summary>
    public ProductVariation AddVariation(
        Sku sku,
        decimal price,
        Barcode? barcode = null,
        decimal? cost = null,
        decimal? weight = null,
        Dimensions? dimensions = null,
        IEnumerable<string>? images = null,
        ProductVariationStatus status = ProductVariationStatus.Active,
        ProductVariationMetadata? metadata = null,
        bool makeDefault = false)
    {
        var displayOrder = Variations.Count == 0 ? 0 : Variations.Max(v => v.DisplayOrder) + 1;

        var variation = ProductVariation.Create(
            Guid.CreateVersion7(), Id, sku, price, displayOrder, barcode, cost, weight, dimensions, images, status, metadata);
        Variations.Add(variation);

        if (makeDefault)
            SetDefaultVariation(variation.Id);

        Tourch();
        return variation;
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

        Tourch();
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
        Tourch();
    }

    public void AssignCategory(Guid categoryId)
    {
        if (CategoryMappings.Any(m => m.CategoryId == categoryId))
            return;

        CategoryMappings.Add(ProductCategoryMapping.Create(Guid.CreateVersion7(), Id, categoryId));
        Tourch();
    }

    public void RemoveCategory(Guid categoryId)
    {
        var mapping = CategoryMappings.FirstOrDefault(m => m.CategoryId == categoryId);
        if (mapping is null)
            return;

        CategoryMappings.Remove(mapping);
        Tourch();
    }

    public void AssignTag(Guid tagId)
    {
        if (TagMappings.Any(m => m.TagId == tagId))
            return;

        TagMappings.Add(ProductTagMapping.Create(Guid.CreateVersion7(), Id, tagId));
        Tourch();
    }

    public void RemoveTag(Guid tagId)
    {
        var mapping = TagMappings.FirstOrDefault(m => m.TagId == tagId);
        if (mapping is null)
            return;

        TagMappings.Remove(mapping);
        Tourch();
    }

    public void UpdateDetails(string name, string description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
        Tourch();
    }

    public void ChangeSlug(Slug slug)
    {
        Slug = slug;
        Tourch();
    }

    public void UpdateMetadata(ProductMetadata metadata)
    {
        Metadata = metadata;
        Tourch();
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Product name cannot be empty.");
    }
}
