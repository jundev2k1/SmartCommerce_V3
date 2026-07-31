namespace Product.Domain.Entities.Categories;

public sealed class ProductCategory : AggregateRoot<Guid>, IAuditable
{
    public CategoryCode Code { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ProductCategoryStatus Status { get; private set; }
    public Guid? ParentCategoryId { get; private set; }

    private ProductCategory() { }

    public static ProductCategory Create(
        Guid id,
        CategoryCode code,
        string name,
        string description,
        Guid? parentCategoryId = null,
        ProductCategoryStatus status = ProductCategoryStatus.Active)
    {
        ValidateName(name);

        if (parentCategoryId == id)
            throw ExceptionFactory.InvalidState("A category cannot be its own parent.");

        return new ProductCategory
        {
            Id = id,
            Code = code,
            Name = name,
            Description = description,
            ParentCategoryId = parentCategoryId,
            Status = status,
        };
    }

    public void UpdateDetails(string name, string description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    /// <summary>
    /// Moves this category under a new parent (or to root when null). Only guards against
    /// direct self-parenting - detecting a deeper ancestor cycle (this category being moved
    /// under one of its own descendants) requires querying the full category tree, which is
    /// an Application-layer concern (repository lookup across many ProductCategory instances),
    /// not something a single aggregate instance can verify on its own.
    /// </summary>
    public void ChangeParent(Guid? parentCategoryId)
    {
        if (parentCategoryId == Id)
            throw ExceptionFactory.InvalidState("A category cannot be its own parent.");

        ParentCategoryId = parentCategoryId;
    }

    public void Activate()
    {
        Status = ProductCategoryStatus.Active;
    }

    public void Deactivate()
    {
        Status = ProductCategoryStatus.Inactive;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Category name cannot be empty.");
    }
}
