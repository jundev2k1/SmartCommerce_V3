using Auth.Persistence.Engine;

using Microsoft.EntityFrameworkCore;

namespace Auth.Persistence.Contexts.Accounts.Repositories;

public sealed class AccountRepo(AuthDbContext dbContext) : IAccountRepository
{
    public async Task DeleteIfExistAsync(Guid id, CancellationToken ct = default)
    {
        await dbContext.Users
            .Where(u => u.Id == id)
            .ExecuteDeleteAsync(ct);
    }
}
