using BuildingBlock.Application.Exceptions;
using BuildingBlock.Persistence.Repository;

namespace User.Persistence.UserProfiles.Repositories;

public sealed class UserProfileRepo(UserDbContext dbContext) : IUserProfileRepository, IRepository<UserProfile>
{
    public async Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<UserProfile?> GetByIdAsync(
        Guid id,
        Func<IQueryable<UserProfile>, IQueryable<UserProfile>> includes,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.UserProfiles);
        return await query.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task AddAsync(UserProfile entity, CancellationToken ct = default)
    {
        await dbContext.UserProfiles.AddAsync(entity, ct);
    }

    public async Task AddRangeAsync(IEnumerable<UserProfile> entities, CancellationToken ct = default)
    {
        await dbContext.UserProfiles.AddRangeAsync(entities, ct);
    }

    public async Task UpdateAsync<TId>(TId id, Func<UserProfile, Task> updateAction, CancellationToken ct = default)
    {
        var user = await dbContext.UserProfiles
            .FirstOrDefaultAsync(u => u.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(id), id!);

        await updateAction(user);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<UserProfile>, IQueryable<UserProfile>> includes,
        Func<UserProfile, Task> updateAction,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.UserProfiles);
        var user = await query.FirstOrDefaultAsync(u => u.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(id), id!);

        await updateAction(user);
    }

    public async Task DeleteAsync<TId>(TId id, CancellationToken ct = default)
    {
        var user = await dbContext.UserProfiles
            .FirstOrDefaultAsync(u => u.Id!.Equals(id), ct);

        if (user is not null)
            dbContext.UserProfiles.Remove(user);
    }

    public async Task DeleteRangeAsync<TId>(TId[] ids, CancellationToken ct = default)
    {
        await dbContext.UserProfiles
            .Where(u => ids.Any(id => id!.Equals(u.Id)))
            .ExecuteDeleteAsync(ct);
    }

    public async Task<int> DeleteWithNoTrackingAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.UserProfiles
            .Where(user => user.Id == id)
            .ExecuteDeleteAsync(ct);
    }
}
