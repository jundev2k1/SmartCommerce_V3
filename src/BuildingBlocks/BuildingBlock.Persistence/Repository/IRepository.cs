namespace BuildingBlock.Persistence.Repository;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<T?> GetByIdAsync(
        Guid id,
        Func<IQueryable<T>, IQueryable<T>> includes,
        CancellationToken ct = default);

    Task AddAsync(T entity, CancellationToken ct = default);

    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    Task UpdateAsync<TId>(
        TId id,
        Func<T, Task> updateAction,
        CancellationToken ct = default);

    Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<T>, IQueryable<T>> includes,
        Func<T, Task> updateAction,
        CancellationToken ct = default);

    Task DeleteAsync<TId>(TId id, CancellationToken ct = default);

    Task DeleteRangeAsync<TId>(TId[] ids, CancellationToken ct = default);
}
