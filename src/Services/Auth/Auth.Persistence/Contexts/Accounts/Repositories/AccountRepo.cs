using SmartEcommerce.Auth.Domain.Entities;
using SmartEcommerce.Auth.Persistence.Engine;

using Microsoft.EntityFrameworkCore;

namespace SmartEcommerce.Auth.Persistence.Contexts.Accounts.Repositories;

public sealed class AccountRepo(AuthDbContext dbContext)
    : AuthBaseRepository<Account>(dbContext), IAccountRepository
{
    public async Task DeleteIfExistAsync(Guid id, CancellationToken ct = default)
    {
        await _dbContext.Users
            .Where(u => u.Id == id)
            .ExecuteDeleteAsync(ct);
    }
}
