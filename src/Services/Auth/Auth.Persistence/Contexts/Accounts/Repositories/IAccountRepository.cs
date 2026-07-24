namespace Auth.Persistence.Contexts.Accounts.Repositories;

public interface IAccountRepository
{
    Task DeleteIfExistAsync(Guid id, CancellationToken ct = default);
}
