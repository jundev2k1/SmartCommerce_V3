using Auth.Domain.Enums;

using BuildingBlock.Domain.Abstractions;
using BuildingBlock.Domain.Attributes;

using Microsoft.AspNetCore.Identity;

namespace Auth.Domain.Entities;

public sealed class Account : IdentityUser<Guid>, IEntity, IAuditable
{
    public UserStatus Status { get; private set; }
    public ICollection<AccountRole> AccountRoles { get; private set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];

    public DateTime CreatedAt { get; set; }

    [AuditIgnore]
    public DateTime UpdatedAt { get; set; }

    private Account() { }

    public static Account Create(string username, string email, UserStatus status = UserStatus.Active)
        => Create(Guid.CreateVersion7(), username, email, status);

    /// <summary>
    /// Creates an Account with an explicit id. Used when the Account must share its
    /// identity with a UserProfile created first in the User service (admin/root-initiated
    /// user creation), rather than generating a fresh id (self-registration).
    /// </summary>
    public static Account Create(Guid id, string username, string email, UserStatus status = UserStatus.Active)
    {
        return new Account
        {
            Id = id,
            UserName = username,
            Email = email,
            EmailConfirmed = false,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void Track()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
    }
}
