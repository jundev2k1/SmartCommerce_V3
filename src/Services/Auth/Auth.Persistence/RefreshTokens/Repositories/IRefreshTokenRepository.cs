using Auth.Domain.Entities;

namespace Auth.Persistence.RefreshTokens.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
}
