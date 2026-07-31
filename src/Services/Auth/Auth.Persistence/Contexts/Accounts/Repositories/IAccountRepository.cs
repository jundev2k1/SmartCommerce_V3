using Auth.Domain.Entities;

using BuildingBlock.Persistence.Repository;

namespace Auth.Persistence.Contexts.Accounts.Repositories;

public interface IAccountRepository : IRepository<Account>
{
    Task DeleteIfExistAsync(Guid id, CancellationToken ct = default);
}
