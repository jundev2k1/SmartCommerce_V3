namespace User.Persistence.UserProfiles.Repositories;

public interface IUserProfileRepository
{
    Task AddAsync(UserProfile entity, CancellationToken ct = default);

    void Remove(UserProfile entity);

    Task<int> DeleteWithNoTrackingAsync(Guid id, CancellationToken ct = default);
}
