namespace NovaCore.Promotion.Domain.Entities.Loyalty;

/// <summary>Per-language override of a LoyaltyProgram's Name/Description - Translation pattern, reuses the parent's Id (Rule 5), composite key (Id, LanguageCode) configured in Persistence.</summary>
public sealed class LoyaltyProgramTranslation : BaseEntity<Guid>, IAuditable, ITenantEntity
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

    private LoyaltyProgramTranslation() { }

    /// <summary>Only LoyaltyProgram may construct a LoyaltyProgramTranslation - see LoyaltyProgram.Translate.</summary>
    internal static LoyaltyProgramTranslation Create(Guid programId, LanguageCode languageCode, string name, string? description)
    {
        ValidateName(name);

        return new LoyaltyProgramTranslation
        {
            Id = programId,
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
            throw ExceptionFactory.RequiredField("Translated loyalty program name cannot be empty.");
    }
}
