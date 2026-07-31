using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Persistence.Repository;

using User.Application.Abstractions.Persistence.UserProfiles;

namespace User.Persistence.Contexts.UserProfiles.Write;

public sealed class UserProfileWriteService(
    IRepository<UserProfile, Guid> repo,
    IUnitOfWork unitOfWork) : IUserProfileWriteService
{
    public async Task<UserProfile> CreateAsync(CreateUserProfileRequest request, CancellationToken ct = default)
    {
        var user = UserProfile.Create(
            request.Id,
            request.Email,
            request.UserName,
            request.PhoneNumber,
            request.FirstName,
            request.MiddleName,
            request.LastName,
            request.Roles);

        await repo.AddAsync(user, ct);

        return user;
    }

    public async Task<UserProfile> SyncFromAccountInitiationAsync(SyncUserProfileRequest request, CancellationToken ct = default)
    {
        var user = UserProfile.Create(
            request.AccountId,
            request.Email,
            request.UserName,
            request.PhoneNumber,
            request.FirstName,
            request.MiddleName,
            request.LastName,
            request.Roles);

        await repo.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return user;
    }

    public async Task UpdateProfileDetailsAsync(Guid id, string firstName, string middleName, string lastName, string phoneNumber, CancellationToken ct = default)
    {
        await repo.UpdateAsync(id, user =>
        {
            user.UpdateProfile(firstName, middleName, lastName, phoneNumber);
        }, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await repo.DeleteByIdAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteWithNoTrackingAsync(Guid id, CancellationToken ct = default)
    {
        return await repo.DeleteWithNoTrackingAsync(u => u.Id == id, ct);
    }
}
