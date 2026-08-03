namespace SmartEcommerce.User.Domain.Entities.Tags;

/// <summary>
/// Owned child of UserTag - a locale-specific override of the tag's display text. UserTag.Name
/// is the internal, language-independent code; DisplayName here is what's shown to users/admins.
/// </summary>
public sealed class UserTagTranslation : BaseEntity<Guid>, IAuditable
{
    public Guid UserTagId { get; private set; }
    public UserTag UserTag { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private UserTagTranslation() { }

    public static UserTagTranslation Create(
        Guid id,
        Guid userTagId,
        LanguageCode languageCode,
        string displayName,
        string? description = null)
    {
        ValidateDisplayName(displayName);

        return new UserTagTranslation
        {
            Id = id,
            UserTagId = userTagId,
            LanguageCode = languageCode,
            DisplayName = displayName,
            Description = description,
        };
    }

    public void UpdateDetails(string displayName, string? description)
    {
        ValidateDisplayName(displayName);

        DisplayName = displayName;
        Description = description;
    }

    public static bool IsValidDisplayName(string? displayName) => !string.IsNullOrWhiteSpace(displayName);

    private static void ValidateDisplayName(string displayName)
    {
        if (!IsValidDisplayName(displayName))
            throw ExceptionFactory.RequiredField("Tag display name cannot be empty.");
    }
}
