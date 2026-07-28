using User.Application.Abstractions.Persistence.UserProfiles;
using User.Persistence.Engine;

namespace User.Persistence.Contexts.UserProfiles.Read;

public sealed class UserProfileReadService(UserDbContext dbContext) : IUserProfileReadService
{
    public async Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<IReadOnlyList<UserProfile>> GetAllAsync(int skip, int take, CancellationToken ct = default)
    {
        return await dbContext.UserProfiles
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }
}
