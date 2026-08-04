using SmartEcommerce.Auth.Domain.Entities.Accounts;
using SmartEcommerce.Auth.Persistence.Engine;

namespace SmartEcommerce.Auth.Persistence.Contexts.RefreshTokens.Repositories;

public sealed class RefreshTokenRepo(AuthDbContext dbContext)
    : AuthBaseRepository<RefreshToken>(dbContext), IRefreshTokenRepository
{
}
