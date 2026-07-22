using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;

namespace User.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PaginatedResult<UserProfile>> SearchAsync(CriteriaRequest request, CancellationToken ct = default);

    Task<UserProfile?> GetByIdAsync(
        Guid id,
        Func<IQueryable<UserProfile>, IQueryable<UserProfile>> includes,
        CancellationToken ct = default);

    Task AddAsync(UserProfile entity, CancellationToken ct = default);

    Task UpdateAsync<TId>(
        TId id,
        Func<UserProfile, Task> updateAction,
        CancellationToken ct = default);
    Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<UserProfile>, IQueryable<UserProfile>> includes,
        Func<UserProfile, Task> updateAction,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task<int> DeleteWithNoTrackingAsync(Guid id, CancellationToken ct = default);
}
