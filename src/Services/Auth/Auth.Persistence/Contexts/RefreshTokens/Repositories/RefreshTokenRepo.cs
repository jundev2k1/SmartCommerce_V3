using Auth.Domain.Entities;
using Auth.Persistence.Engine;

namespace Auth.Persistence.Contexts.RefreshTokens.Repositories;

public sealed class RefreshTokenRepo(AuthDbContext dbContext)
    : AuthBaseRepository<RefreshToken>(dbContext), IRefreshTokenRepository
{
}
