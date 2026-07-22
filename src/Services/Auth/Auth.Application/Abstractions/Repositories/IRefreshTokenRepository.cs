using Auth.Domain.Entities;

namespace Auth.Application.Abstractions.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<RefreshToken?> GetByIdAsync(
        Guid id,
        Func<IQueryable<RefreshToken>, IQueryable<RefreshToken>> includes,
        CancellationToken ct = default);

    Task<RefreshToken?> GetByJwtIdAsync(Guid jwtId, CancellationToken ct = default);

    Task<RefreshToken?> GetByJwtIdAsync(
        Guid jwtId,
        Func<IQueryable<RefreshToken>, IQueryable<RefreshToken>> includes,
        CancellationToken ct = default);

    Task<List<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task<RefreshToken?> GetValidTokenAsync(Guid userId, string token, CancellationToken ct = default);

    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);

    Task AddAsync(RefreshToken token, CancellationToken ct = default);

    Task UpdateAsync(Guid id, Func<RefreshToken, Task> updateAction, CancellationToken ct = default);

    Task UpdateAsync(
        Guid id,
        Func<IQueryable<RefreshToken>, IQueryable<RefreshToken>> includes,
        Func<RefreshToken, Task> updateAction,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task RevokeByUserIdAsync(Guid userId, CancellationToken ct = default);
}
