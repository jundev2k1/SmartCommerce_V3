using Auth.Domain.Entities;
using Auth.Persistence.Engine;
using BuildingBlock.Domain.Seeders;
using Microsoft.EntityFrameworkCore;

namespace Auth.Persistence.Seeders;

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
