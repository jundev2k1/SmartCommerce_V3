using BuildingBlock.Domain.Abstractions;

using Microsoft.AspNetCore.Identity;

namespace Auth.Domain.Entities;

public class AccountRole : IdentityUserRole<Guid>, IEntity
{
    public virtual Account? Account { get; set; }
    public virtual Role? Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public void Tourch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
