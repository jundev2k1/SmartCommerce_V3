using Auth.Domain.Entities;

namespace Auth.Application.Abstractions.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Account?> GetByIdAsync(
        Guid id,
        Func<IQueryable<Account>, IQueryable<Account>> includes,
        CancellationToken ct = default);

    Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task<Account?> GetByEmailAsync(
        string email,
        Func<IQueryable<Account>, IQueryable<Account>> includes,
        CancellationToken ct = default);

    Task<List<Account>> GetActiveUsersAsync(CancellationToken ct = default);

    Task AddAsync(Account account, CancellationToken ct = default);

    Task UpdateAsync(Guid id, Func<Account, Task> updateAction, CancellationToken ct = default);

    Task UpdateAsync(
        Guid id,
        Func<IQueryable<Account>, IQueryable<Account>> includes,
        Func<Account, Task> updateAction,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task DeleteIfExistAsync(Guid id, CancellationToken ct = default);
}
