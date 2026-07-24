using Auth.Application.Abstractions.Persistence.RefreshTokens;
using Auth.Domain.Entities;
using Auth.Persistence.Engine;

using Microsoft.EntityFrameworkCore;

namespace Auth.Persistence.Contexts.RefreshTokens.Read;

public sealed class RefreshTokenReadService(AuthDbContext dbContext) : IRefreshTokenReadService
{
    public async Task<List<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await dbContext.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.AccountId == userId && !rt.IsRevoked)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(ct);
    }
}
