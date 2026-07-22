namespace Product.Domain.Entities;

public sealed class ProductTag : AggregateRoot<Guid>, IAuditable
{
    public TagCode Code { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;

    private ProductTag() { }

    public static ProductTag Create(Guid id, TagCode code, string name)
    {
        ValidateName(name);

        return new ProductTag
        {
            Id = id,
            Code = code,
            Name = name,
        };
    }

    public void Rename(string name)
    {
        ValidateName(name);

        Name = name;
        Tourch();
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Tag name cannot be empty.");
    }
}
