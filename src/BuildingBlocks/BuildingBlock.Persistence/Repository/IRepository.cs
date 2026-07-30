using System.Linq.Expressions;

using BuildingBlock.Domain.Abstractions;

namespace BuildingBlock.Persistence.Repository;

public interface IRepository<TEntity> where TEntity : IEntity
{
    Task<TEntity?> GetByIdAsync<TId>(TId id, CancellationToken ct = default);

    Task<TEntity?> GetByIdAsync<TId>(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        CancellationToken ct = default);

    Task AddAsync(TEntity entity, CancellationToken ct = default);

    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

    Task UpdateAsync<TId>(
        TId id,
        Action<TEntity> updateAction,
        CancellationToken ct = default);
    Task UpdateAsync<TId>(
        TId id,
        Func<TEntity, Task> updateAction,
        CancellationToken ct = default);
    Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        Action<TEntity> updateAction,
        CancellationToken ct = default);
    Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        Func<TEntity, Task> updateAction,
        CancellationToken ct = default);

    Task DeleteAsync<TId>(TId id, CancellationToken ct = default);

    Task DeleteRangeAsync<TId>(TId[] ids, CancellationToken ct = default);

    Task DeleteWithNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default);
}
