using SmartEcommerce.Auth.Domain.Entities.Accounts;
using SmartEcommerce.Auth.Domain.Enums;
using SmartEcommerce.Auth.Persistence.Engine;

using SmartEcommerce.BuildingBlock.Domain.Seeders;
using SmartEcommerce.BuildingBlock.Domain.ValueObjects;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SmartEcommerce.Auth.Persistence.Storage.Seeders;

public class AccountSeeder(AuthDbContext context, UserManager<Account> userManager)
{
    public async Task SeedAsync()
    {
        if (await context.Users.AnyAsync())
            return;

        foreach (var (id, username, email, password, roles) in SeedAuthData.Accounts.Default)
        {
            var account = Account.Create(username, Email.Create(email), AccountStatus.Active);
            account.Id = id;
            account.ConfirmEmail();

            var result = await userManager.CreateAsync(account, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create account '{username}': {string.Join(", ", result.Errors.Select(e => e.Description))}"
                );
            }

            foreach (var role in roles)
            {
                var roleResult = await userManager.AddToRoleAsync(account, role);
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to assign role '{role}' to account '{username}'"
                    );
                }
            }
        }

        await context.SaveChangesAsync();
    }
}
