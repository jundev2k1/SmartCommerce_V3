using SmartEcommerce.Auth.Domain.Entities;
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
            .Select(r => Role.Create(r.Name, r.Description))
            .ToList();

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
    }
}
