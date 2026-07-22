using BuildingBlock.Domain.Seeders;

namespace User.Persistence.Seeders;

public sealed class UserSeeder(UserDbContext context)
{
    public async Task SeedAsync()
    {
        if (await context.UserProfiles.AnyAsync())
            return;

        var users = SeedAuthData.Accounts.Default
            .Select(account => UserProfile.Create(
                account.Id,
                account.Email,
                account.Username,
                "1234567890",
                account.Username,
                account.Username))
            .ToArray();

        context.UserProfiles.AddRange(users);
        await context.SaveChangesAsync();
    }
}
