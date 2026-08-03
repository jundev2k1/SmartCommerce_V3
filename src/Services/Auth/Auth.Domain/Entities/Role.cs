using SmartEcommerce.BuildingBlock.Domain.Abstractions;
using SmartEcommerce.BuildingBlock.Domain.Attributes;

using Microsoft.AspNetCore.Identity;

namespace SmartEcommerce.Auth.Domain.Entities;

public sealed class Role : IdentityRole<Guid>, IEntity, IAuditable
{
    public string? Description { get; set; }
    public ICollection<AccountRole> UserRoles { get; private set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [AuditIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static Role Create(string name, string? description = null)
    {
        return new Role
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            NormalizedName = name.ToUpper(),
            Description = description,
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
}
