using Auth.Domain.Entities;

namespace Auth.Persistence.RefreshTokens.Repositories;

public sealed class RefreshTokenRepo(AuthDbContext dbContext) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        await dbContext.RefreshTokens.AddAsync(token, ct);
    }
}
