namespace Product.Application.Abstractions.Repositories;

public interface IProductCategoryRepository
{
    Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(ProductCategory entity, CancellationToken ct = default);

    Task UpdateAsync<TId>(
        TId id,
        Func<ProductCategory, Task> updateAction,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);

    Task<bool> HasChildrenAsync(Guid categoryId, CancellationToken ct = default);

    Task<IReadOnlyList<Guid>> GetChildIdsAsync(Guid categoryId, CancellationToken ct = default);

    Task<IReadOnlyList<ProductCategory>> GetAllAsync(CancellationToken ct = default);
}
