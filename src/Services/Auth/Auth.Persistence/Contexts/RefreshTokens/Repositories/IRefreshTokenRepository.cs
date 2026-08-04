using SmartEcommerce.Auth.Domain.Entities.Accounts;

using SmartEcommerce.BuildingBlock.Persistence.Repository;

namespace SmartEcommerce.Auth.Persistence.Contexts.RefreshTokens.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
