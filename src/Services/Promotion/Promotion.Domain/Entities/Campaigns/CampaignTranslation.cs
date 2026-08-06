namespace NovaCore.Promotion.Domain.Entities.Campaigns;

/// <summary>Per-language override of a Campaign's Name/Description - Translation pattern, reuses the parent's Id (Rule 5), composite key (Id, LanguageCode) configured in Persistence. Renamed from CampaignLocalization during the Phase 2.5 Domain Standardization Review for naming consistency with every other Translation entity in the platform (ProductBrandTranslation, RoleTranslation, ...).</summary>
public sealed class CampaignTranslation : BaseEntity<Guid>, IAuditable, ITenantEntity
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

    private CampaignTranslation() { }

    /// <summary>Only Campaign may construct a CampaignTranslation - see Campaign.Translate.</summary>
    internal static CampaignTranslation Create(Guid campaignId, LanguageCode languageCode, string name, string? description)
    {
        ValidateName(name);

        return new CampaignTranslation
        {
            Id = campaignId,
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
            throw ExceptionFactory.RequiredField("Translated campaign name cannot be empty.");
    }
}
