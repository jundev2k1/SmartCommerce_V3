using Auth.Application.Abstractions.Persistence.RefreshTokens;
using Auth.Domain.Entities;
using Auth.Persistence.RefreshTokens.Repositories;

using BuildingBlock.Application.Exceptions;

using Microsoft.EntityFrameworkCore;

namespace Auth.Persistence.RefreshTokens.Write;

public sealed class RefreshTokenWriteService(
    AuthDbContext dbContext,
    IRefreshTokenRepository repo) : IRefreshTokenWriteService
{
    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        await repo.AddAsync(token, ct);
    }

    public async Task UpdateAsync(Guid id, Func<RefreshToken, Task> updateAction, CancellationToken ct = default)
    {
        var token = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Id == id, ct)
            ?? throw new NotFoundException(nameof(id), id);

        await updateAction(token);
    }
}
