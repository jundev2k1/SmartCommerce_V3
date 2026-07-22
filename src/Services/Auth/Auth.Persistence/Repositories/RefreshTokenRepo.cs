using Auth.Application.Abstractions.Repositories;
using Auth.Domain.Entities;

using BuildingBlock.Application.Exceptions;
using BuildingBlock.Persistence.Repository;

using Microsoft.EntityFrameworkCore;

namespace Auth.Persistence.Repositories;

public sealed class RefreshTokenRepo(AuthDbContext dbContext) : IRepository<RefreshToken>, IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await GetByIdAsync(id, q => q.Include(rt => rt.Account), ct);
    }

    public async Task<RefreshToken?> GetByIdAsync(
        Guid id,
        Func<IQueryable<RefreshToken>, IQueryable<RefreshToken>> includes,
        CancellationToken ct = default)
    {
        return await includes(dbContext.RefreshTokens.AsNoTracking())
            .FirstOrDefaultAsync(rt => rt.Id == id, ct);
    }

    public async Task<RefreshToken?> GetByJwtIdAsync(Guid jwtId, CancellationToken ct = default)
    {
        return await GetByJwtIdAsync(jwtId, q => q.Include(rt => rt.Account), ct);
    }

    public async Task<RefreshToken?> GetByJwtIdAsync(
        Guid jwtId,
        Func<IQueryable<RefreshToken>, IQueryable<RefreshToken>> includes,
        CancellationToken ct = default)
    {
        return await includes(dbContext.RefreshTokens.AsNoTracking())
            .FirstOrDefaultAsync(rt => rt.JwtId == jwtId, ct);
    }

    public async Task<List<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await dbContext.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.AccountId == userId && !rt.IsRevoked)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<RefreshToken?> GetValidTokenAsync(Guid userId, string token, CancellationToken ct = default)
    {
        return await dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt =>
                rt.AccountId == userId &&
                rt.Token == token &&
                !rt.IsRevoked &&
                rt.ExpiryDate > DateTime.UtcNow, ct);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return await dbContext.RefreshTokens
            .AsNoTracking()
            .Include(rt => rt.Account)
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);
    }

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        await dbContext.RefreshTokens.AddAsync(token, ct);
    }

    public async Task UpdateAsync<TId>(TId id, Func<RefreshToken, Task> updateAction, CancellationToken ct = default)
    {
        if (id is Guid guidId)
            await UpdateAsync(guidId, q => q, updateAction, ct);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<RefreshToken>, IQueryable<RefreshToken>> includes,
        Func<RefreshToken, Task> updateAction,
        CancellationToken ct = default)
    {
        if (id is Guid guidId)
        {
            var token = await includes(dbContext.RefreshTokens)
                .FirstOrDefaultAsync(rt => rt.Id == guidId, ct)
                ?? throw new NotFoundException(nameof(id), guidId);

            await updateAction(token);
        }
    }

    public async Task UpdateAsync(Guid id, Func<RefreshToken, Task> updateAction, CancellationToken ct = default)
    {
        await UpdateAsync<Guid>(id, q => q, updateAction, ct);
    }

    public async Task UpdateAsync(
        Guid id,
        Func<IQueryable<RefreshToken>, IQueryable<RefreshToken>> includes,
        Func<RefreshToken, Task> updateAction,
        CancellationToken ct = default)
    {
        await UpdateAsync<Guid>(id, includes, updateAction, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var token = await dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Id == id, ct)
            ?? throw new NotFoundException(nameof(id), id);

        dbContext.RefreshTokens.Remove(token);
    }

    public async Task RevokeByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var tokens = await dbContext.RefreshTokens
            .Where(rt => rt.AccountId == userId && !rt.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in tokens)
        {
            token.Revoke();
        }
    }
}
