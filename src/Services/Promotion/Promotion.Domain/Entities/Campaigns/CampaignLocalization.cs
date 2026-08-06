namespace NovaCore.Promotion.Domain.Entities.Campaigns;

/// <summary>Per-language override of a Campaign's Name/Description - Translation pattern, reuses the parent's Id (Rule 5), composite key (Id, LanguageCode) configured in Persistence.</summary>
public sealed class CampaignLocalization : BaseEntity<Guid>, IAuditable, ITenantEntity
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

    private CampaignLocalization() { }

    /// <summary>Only Campaign may construct a CampaignLocalization - see Campaign.Translate.</summary>
    internal static CampaignLocalization Create(Guid campaignId, LanguageCode languageCode, string name, string? description)
    {
        ValidateName(name);

        return new CampaignLocalization
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
            throw ExceptionFactory.RequiredField("Localized campaign name cannot be empty.");
    }
}
