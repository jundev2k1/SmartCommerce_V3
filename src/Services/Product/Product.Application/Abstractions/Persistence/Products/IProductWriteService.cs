namespace Product.Application.Abstractions.Persistence.Products;

public interface IProductWriteService
{
    Task CreateAsync(ProductEntity product, CancellationToken ct = default);

    /// <summary>
    /// Self-committing. Covers every Product mutation whose outbox event payload doesn't depend
    /// on the mutation's own output (details, variation update/delete/reorder, category/tag
    /// assign/remove) - the outbox enqueue can be staged before this call per Correction 1a.
    /// </summary>
    Task UpdateAsync(Guid id, Func<ProductEntity, Task> updateAction, CancellationToken ct = default);

    /// <summary>
    /// Non-committing. Used only by AddVariation, whose outbox event needs the newly-generated
    /// variation's Id/Sku/etc - those only exist after updateAction runs, so the enqueue can't be
    /// staged beforehand (Correction 1a doesn't apply) and the caller must own one
    /// ExecuteTransactionAsync spanning both the mutation and the enqueue. Same non-committing
    /// shape as Correction 2, for a different reason (output-dependent side effect, not
    /// cross-aggregate) - see the persistence refactor tracker.
    /// </summary>
    Task StageUpdateAsync(Guid id, Func<ProductEntity, Task> updateAction, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
