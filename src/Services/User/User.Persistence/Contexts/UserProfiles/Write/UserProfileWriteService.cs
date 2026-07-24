using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Persistence.Repository;

using User.Application.Abstractions.Persistence.UserProfiles;
using User.Persistence.Contexts.UserProfiles.Repositories;

namespace User.Persistence.Contexts.UserProfiles.Write;

/// <summary>
/// CreateAsync/UpdateProfileDetailsAsync never call IUnitOfWork themselves - the caller
/// (CreateUserHandler/UpdateUserHandler) owns ExecuteTransactionAsync. SyncFromAccountInitiationAsync
/// and DeleteAsync commit via bare SaveChangesAsync themselves, matching their original handlers'
/// shape (OnUserInitiatedHandler/DeleteUserHandler never used ExecuteTransactionAsync).
/// </summary>
public sealed class UserProfileWriteService(
    IRepository<UserProfile> repo,
    IUserProfileRepository userProfileRepo,
    IUnitOfWork unitOfWork) : IUserProfileWriteService
{
    public async Task CreateAsync(UserProfile user, CancellationToken ct = default)
    {
        await repo.AddAsync(user, ct);
    }

    public async Task SyncFromAccountInitiationAsync(UserProfile user, CancellationToken ct = default)
    {
        await repo.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateProfileDetailsAsync(Guid id, string firstName, string lastName, string phoneNumber, CancellationToken ct = default)
    {
        await repo.UpdateAsync(id, async user =>
        {
            user.UpdateProfile(firstName, lastName, phoneNumber);
            await Task.CompletedTask;
        }, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await repo.DeleteAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteWithNoTrackingAsync(Guid id, CancellationToken ct = default)
    {
        return await userProfileRepo.DeleteWithNoTrackingAsync(id, ct);
    }
}
