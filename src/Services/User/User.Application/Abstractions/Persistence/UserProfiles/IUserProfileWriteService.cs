namespace User.Application.Abstractions.Persistence.UserProfiles;

public interface IUserProfileWriteService
{
    /// <summary>Used by CreateUser: commits via ExecuteTransactionAsync (ConflictException on unique-index races).</summary>
    Task CreateAsync(UserProfile user, CancellationToken ct = default);

    /// <summary>Used by OnUserInitiated: mirrors an Account created in Auth, commits via bare SaveChangesAsync.</summary>
    Task SyncFromAccountInitiationAsync(UserProfile user, CancellationToken ct = default);

    Task UpdateProfileDetailsAsync(Guid id, string firstName, string lastName, string phoneNumber, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task<int> DeleteWithNoTrackingAsync(Guid id, CancellationToken ct = default);
}
