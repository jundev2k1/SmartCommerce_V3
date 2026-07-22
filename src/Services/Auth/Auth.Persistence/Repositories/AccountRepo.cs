using Auth.Application.Abstractions.Repositories;
using Auth.Domain.Entities;
using Auth.Domain.Enums;

using BuildingBlock.Application.Exceptions;
using BuildingBlock.Persistence.Repository;

using Microsoft.EntityFrameworkCore;

namespace Auth.Persistence.Repositories;

public sealed class AccountRepo(AuthDbContext dbContext) : IRepository<Account>, IAccountRepository
{
    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await GetByIdAsync(id, q => q.Include(u => u.AccountRoles), ct);
    }

    public async Task<Account?> GetByIdAsync(
        Guid id,
        Func<IQueryable<Account>, IQueryable<Account>> includes,
        CancellationToken ct = default)
    {
        return await includes(dbContext.Users.AsNoTracking())
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await GetByEmailAsync(email, q => q.Include(u => u.AccountRoles), ct);
    }

    public async Task<Account?> GetByEmailAsync(
        string email,
        Func<IQueryable<Account>, IQueryable<Account>> includes,
        CancellationToken ct = default)
    {
        return await includes(dbContext.Users.AsNoTracking())
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<List<Account>> GetActiveUsersAsync(CancellationToken ct = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Status == UserStatus.Active)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Account account, CancellationToken ct = default)
    {
        await dbContext.Users.AddAsync(account, ct);
    }

    public async Task UpdateAsync<TId>(TId id, Func<Account, Task> updateAction, CancellationToken ct = default)
    {
        if (id is Guid guidId)
            await UpdateAsync(guidId, q => q, updateAction, ct);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<Account>, IQueryable<Account>> includes,
        Func<Account, Task> updateAction,
        CancellationToken ct = default)
    {
        if (id is Guid guidId)
            await UpdateAsync(guidId, includes, updateAction, ct);
    }

    async Task IAccountRepository.UpdateAsync(
        Guid id,
        Func<Account, Task> updateAction,
        CancellationToken ct)
    {
        var account = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException(nameof(id), id);

        await updateAction(account);
    }

    async Task IAccountRepository.UpdateAsync(
        Guid id,
        Func<IQueryable<Account>, IQueryable<Account>> includes,
        Func<Account, Task> updateAction,
        CancellationToken ct)
    {
        var account = await includes(dbContext.Users)
            .FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException(nameof(id), id);

        await updateAction(account);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var account = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException(nameof(id), id);

        dbContext.Users.Remove(account);
    }

    public async Task DeleteIfExistAsync(Guid id, CancellationToken ct = default)
    {
        await dbContext.Users
            .Where(u => u.Id == id)
            .ExecuteDeleteAsync(ct);
    }
}
