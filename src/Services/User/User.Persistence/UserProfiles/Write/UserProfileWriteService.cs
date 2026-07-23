using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Application.Exceptions;

using User.Application.Abstractions.Persistence.UserProfiles;
using User.Persistence.UserProfiles.Repositories;

namespace User.Persistence.UserProfiles.Write;

public sealed class UserProfileWriteService(
    UserDbContext dbContext,
    IUserProfileRepository repo,
    IUnitOfWork unitOfWork) : IUserProfileWriteService
{
    public async Task CreateAsync(UserProfile user, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await repo.AddAsync(user, ct);
        }, ct: ct);
    }

    public async Task SyncFromAccountInitiationAsync(UserProfile user, CancellationToken ct = default)
    {
        await repo.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateProfileAsync(Guid id, Func<UserProfile, Task> updateAction, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var user = await dbContext.UserProfiles
                .FirstOrDefaultAsync(u => u.Id == id, ct)
                ?? throw new NotFoundException(nameof(id), id);

            await updateAction(user);
        }, ct: ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await dbContext.UserProfiles
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        if (user is not null)
            repo.Remove(user);

        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteWithNoTrackingAsync(Guid id, CancellationToken ct = default)
    {
        return await repo.DeleteWithNoTrackingAsync(id, ct);
    }
}
