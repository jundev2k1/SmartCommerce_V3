using SmartEcommerce.Auth.Domain.Entities.Roles;
using SmartEcommerce.Auth.Domain.ValueObjects;
using SmartEcommerce.Auth.Persistence.Engine;

using SmartEcommerce.BuildingBlock.Domain.Seeders;

using Microsoft.EntityFrameworkCore;

namespace SmartEcommerce.Auth.Persistence.Storage.Seeders;

public class RoleSeeder(AuthDbContext context)
{
    public async Task SeedAsync()
    {
        if (await context.Roles.AnyAsync())
            return;

        var roles = SeedAuthData.Roles.Default
            .Select(r => Role.Create(r.Name, RoleCode.Create(r.Name), r.Description, isSystemRole: true))
            .ToList();

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
    }
}
