using Auth.Application.Abstractions.Persistence.Accounts;
using Auth.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Auth.Persistence.Accounts.Read;

public sealed class AccountReadService(AuthDbContext dbContext) : IAccountReadService
{
    public async Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Include(u => u.AccountRoles)
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }
}
