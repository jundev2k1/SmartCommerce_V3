namespace User.Persistence.UserProfiles.Repositories;

public sealed class UserProfileRepo(UserDbContext dbContext) : IUserProfileRepository
{
    public async Task AddAsync(UserProfile entity, CancellationToken ct = default)
    {
        await dbContext.UserProfiles.AddAsync(entity, ct);
    }

    public void Remove(UserProfile entity)
    {
        dbContext.UserProfiles.Remove(entity);
    }

    public async Task<int> DeleteWithNoTrackingAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.UserProfiles
            .Where(user => user.Id == id)
            .ExecuteDeleteAsync(ct);
    }
}
