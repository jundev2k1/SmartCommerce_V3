namespace NovaCore.Promotion.Domain.Entities.ProductSets;

/// <summary>Per-language override of a ProductSet's Name/Description - Translation pattern, reuses the parent's Id (Rule 5), composite key (Id, LanguageCode) configured in Persistence.</summary>
public sealed class ProductSetTranslation : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductSetTranslation() { }

    /// <summary>Only ProductSet may construct a ProductSetTranslation - see ProductSet.Translate.</summary>
    internal static ProductSetTranslation Create(Guid productSetId, LanguageCode languageCode, string name, string? description)
    {
        ValidateName(name);

        return new ProductSetTranslation
        {
            Id = productSetId,
            LanguageCode = languageCode,
            Name = name,
            Description = description,
        };
    }

    internal void UpdateDetails(string name, string? description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Translated product set name cannot be empty.");
    }
}
