namespace Product.Application.Abstractions.Repositories;

public interface IProductTagRepository
{
    Task<ProductTag?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(ProductTag entity, CancellationToken ct = default);

    Task UpdateAsync<TId>(
        TId id,
        Func<ProductTag, Task> updateAction,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);

    Task<IReadOnlyList<ProductTag>> GetAllAsync(CancellationToken ct = default);
}
