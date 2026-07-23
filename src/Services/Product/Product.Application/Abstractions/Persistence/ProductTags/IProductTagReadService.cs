namespace Product.Application.Abstractions.Persistence.ProductTags;

public interface IProductTagReadService
{
    Task<ProductTag?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);

    Task<IReadOnlyList<ProductTag>> GetAllAsync(CancellationToken ct = default);
}
