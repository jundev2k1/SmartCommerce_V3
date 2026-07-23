using Auth.Application.Abstractions.Persistence.Accounts;
using Auth.Persistence.Accounts.Repositories;

using BuildingBlock.Application.Abstractions.Persistence;

namespace Auth.Persistence.Accounts.Write;

public sealed class AccountWriteService(
    IAccountRepository repo,
    IUnitOfWork unitOfWork) : IAccountWriteService
{
    public async Task DeleteIfExistAsync(Guid id, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await repo.DeleteIfExistAsync(id, ct);
        }, ct: ct);
    }
}
