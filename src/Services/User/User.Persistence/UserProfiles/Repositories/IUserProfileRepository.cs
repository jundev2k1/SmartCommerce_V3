namespace User.Persistence.UserProfiles.Repositories;

public interface IUserProfileRepository
{
    /// <summary>Bulk ExecuteDeleteAsync - bypasses the change tracker entirely, doesn't fit the generic IRepository&lt;T&gt; shape.</summary>
    Task<int> DeleteWithNoTrackingAsync(Guid id, CancellationToken ct = default);
}
