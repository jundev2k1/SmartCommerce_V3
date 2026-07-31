using Auth.Domain.Entities;

using BuildingBlock.Persistence.Repository;

namespace Auth.Persistence.Contexts.RefreshTokens.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
