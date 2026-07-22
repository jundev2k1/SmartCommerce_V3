using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Persistence.Ef.Criteria;
using BuildingBlock.Persistence.Repository;

using User.Application.Abstractions.Repositories;
using User.Application.Features.Users.Search;

namespace User.Persistence.Repository;

public sealed class UserRepository(UserDbContext dbContext) : IUserRepository, IRepository<UserProfile>
{
    public async Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<PaginatedResult<UserProfile>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await dbContext.UserProfiles
            .AsNoTracking()
            .ApplyCriteria(UserCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
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

    public async Task UpdateAsync<TId>(
        TId id,
        Func<UserProfile, Task> updateAction,
        CancellationToken ct = default)
    {
        var user = await dbContext.UserProfiles
            .FirstOrDefaultAsync(u => u.Id.Equals(id), ct)
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
        var user = await query.FirstOrDefaultAsync(u => u.Id.Equals(id), ct)
            ?? throw new NotFoundException(nameof(id), id!);

        await updateAction(user);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await dbContext.UserProfiles
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        if (user is not null)
            dbContext.UserProfiles.Remove(user);
    }

    public async Task<int> DeleteWithNoTrackingAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.UserProfiles
            .Where(user => user.Id == id)
            .ExecuteDeleteAsync(ct);
    }
}
