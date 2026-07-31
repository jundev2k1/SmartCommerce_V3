namespace User.Application.Abstractions.Persistence.UserProfiles;

public sealed record CreateUserProfileRequest(
    Guid Id,
    string Email,
    string UserName,
    string PhoneNumber,
    string FirstName,
    string MiddleName,
    string LastName,
    string[] Roles);

public sealed record SyncUserProfileRequest(
    Guid AccountId,
    string Email,
    string UserName,
    string PhoneNumber,
    string FirstName,
    string MiddleName,
    string LastName,
    string[] Roles);

public interface IUserProfileWriteService
{
    /// <summary>Used by CreateUser: commits via ExecuteTransactionAsync (ConflictException on unique-index races). Returns the created UserProfile - CreateUserHandler needs it to build UserProfileCreatedIntegrationEvent.</summary>
    Task<UserProfile> CreateAsync(CreateUserProfileRequest request, CancellationToken ct = default);

    /// <summary>Used by OnUserInitiated: mirrors an Account created in Auth, commits via bare SaveChangesAsync. Returns the created UserProfile - OnUserInitiatedHandler needs its Id for OnUserSearchSyncRequiredEvent.</summary>
    Task<UserProfile> SyncFromAccountInitiationAsync(SyncUserProfileRequest request, CancellationToken ct = default);

    Task UpdateProfileDetailsAsync(Guid id, string firstName, string middleName, string lastName, string phoneNumber, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task<int> DeleteWithNoTrackingAsync(Guid id, CancellationToken ct = default);
}
