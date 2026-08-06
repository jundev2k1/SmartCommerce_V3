namespace NovaCore.Promotion.Domain.Entities.ProductSets;

/// <summary>Per-language override of a ProductBundle's Name - Translation pattern, reuses the parent's Id (Rule 5), composite key (Id, LanguageCode) configured in Persistence. Name-only, since ProductBundle itself has no Description field.</summary>
public sealed class ProductBundleTranslation : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductBundleTranslation() { }

    /// <summary>Only ProductBundle may construct a ProductBundleTranslation - see ProductBundle.Translate.</summary>
    internal static ProductBundleTranslation Create(Guid bundleId, LanguageCode languageCode, string name)
    {
        ValidateName(name);

        return new ProductBundleTranslation
        {
            Id = bundleId,
            LanguageCode = languageCode,
            Name = name,
        };
    }

    internal void UpdateDetails(string name)
    {
        ValidateName(name);

        Name = name;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Translated bundle name cannot be empty.");
    }
}
