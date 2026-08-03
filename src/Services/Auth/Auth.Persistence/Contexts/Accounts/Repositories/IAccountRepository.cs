using SmartEcommerce.Auth.Domain.Entities;

using SmartEcommerce.BuildingBlock.Persistence.Repository;

namespace SmartEcommerce.Auth.Persistence.Contexts.Accounts.Repositories;

public interface IAccountRepository : IRepository<Account>
{
    Task DeleteIfExistAsync(Guid id, CancellationToken ct = default);
}
