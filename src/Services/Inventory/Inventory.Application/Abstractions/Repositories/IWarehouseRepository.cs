namespace Inventory.Application.Abstractions.Repositories;

public interface IWarehouseRepository
{
    Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Warehouse?> GetByIdAsync(
        Guid id,
        Func<IQueryable<Warehouse>, IQueryable<Warehouse>> includes,
        CancellationToken ct = default);

    Task<Warehouse?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task AddAsync(Warehouse entity, CancellationToken ct = default);

    Task UpdateAsync<TId>(
        TId id,
        Func<Warehouse, Task> updateAction,
        CancellationToken ct = default);

    Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<Warehouse>, IQueryable<Warehouse>> includes,
        Func<Warehouse, Task> updateAction,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
