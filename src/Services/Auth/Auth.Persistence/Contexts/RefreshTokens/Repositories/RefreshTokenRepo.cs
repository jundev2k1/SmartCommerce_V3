using Auth.Domain.Entities;
using Auth.Persistence.Engine;

using BuildingBlock.Application.Exceptions;
using BuildingBlock.Persistence.Repository;

using Microsoft.EntityFrameworkCore;

namespace Auth.Persistence.Contexts.RefreshTokens.Repositories;

public sealed class RefreshTokenRepo(AuthDbContext dbContext) : IRefreshTokenRepository, IRepository<RefreshToken>
{
    public async Task<RefreshToken?> GetByIdAsync<TId>(TId id, CancellationToken ct = default)
    {
        return await dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Id.Equals(id), ct);
    }

    public async Task<RefreshToken?> GetByIdAsync<TId>(
        TId id,
        Func<IQueryable<RefreshToken>, IQueryable<RefreshToken>> includes,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.RefreshTokens);
        return await query.FirstOrDefaultAsync(rt => rt.Id.Equals(id), ct);
    }

    public async Task AddAsync(RefreshToken entity, CancellationToken ct = default)
    {
        await dbContext.RefreshTokens.AddAsync(entity, ct);
    }

    public async Task AddRangeAsync(IEnumerable<RefreshToken> entities, CancellationToken ct = default)
    {
        await dbContext.RefreshTokens.AddRangeAsync(entities, ct);
    }

    public async Task UpdateAsync<TId>(TId id, Func<RefreshToken, Task> updateAction, CancellationToken ct = default)
    {
        var token = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(id), id!);

        await updateAction(token);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<RefreshToken>, IQueryable<RefreshToken>> includes,
        Func<RefreshToken, Task> updateAction,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.RefreshTokens);
        var token = await query.FirstOrDefaultAsync(rt => rt.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(id), id!);

        await updateAction(token);
    }

    public async Task DeleteAsync<TId>(TId id, CancellationToken ct = default)
    {
        var token = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Id!.Equals(id), ct);

        if (token is not null)
            dbContext.RefreshTokens.Remove(token);
    }

    public async Task DeleteRangeAsync<TId>(TId[] ids, CancellationToken ct = default)
    {
        await dbContext.RefreshTokens
            .Where(rt => ids.Any(id => id!.Equals(rt.Id)))
            .ExecuteDeleteAsync(ct);
    }
}
