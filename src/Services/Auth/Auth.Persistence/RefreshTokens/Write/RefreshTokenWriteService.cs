using Auth.Application.Abstractions.Persistence.RefreshTokens;
using Auth.Domain.Entities;

using BuildingBlock.Persistence.Repository;

namespace Auth.Persistence.RefreshTokens.Write;

/// <summary>
/// Both methods are non-committing - the one caller (RefreshTokenSyncService) batches many
/// Add/Update calls across a whole user-batch into one IUnitOfWork.ExecuteTransactionAsync it
/// owns itself. See the persistence refactor tracker's extension of Correction 2 to Auth.
/// </summary>
public sealed class RefreshTokenWriteService(
    IRepository<RefreshToken> repo) : IRefreshTokenWriteService
{
    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        await repo.AddAsync(token, ct);
    }

    public async Task UpdateAsync(Guid id, Func<RefreshToken, Task> updateAction, CancellationToken ct = default)
    {
        await repo.UpdateAsync(id, updateAction, ct);
    }
}
