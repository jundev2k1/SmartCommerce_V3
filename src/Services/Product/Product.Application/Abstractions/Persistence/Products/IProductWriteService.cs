using Product.Domain.ValueObjects;

namespace Product.Application.Abstractions.Persistence.Products;

public interface IProductWriteService
{
    Task CreateAsync(ProductEntity product, CancellationToken ct = default);

    Task UpdateDetailsAsync(Guid id, string name, string description, string slug, CancellationToken ct = default);

    Task<ProductVariation> AddVariationAsync(
        Guid productId,
        Sku sku,
        decimal price,
        Barcode? barcode,
        decimal? cost,
        decimal? weight,
        Dimensions? dimensions,
        IEnumerable<string>? images,
        bool makeDefault,
        CancellationToken ct = default);

    Task UpdateVariationInformationAsync(
        Guid productId,
        Guid variationId,
        Sku sku,
        decimal price,
        Barcode? barcode,
        decimal? cost,
        decimal? weight,
        Dimensions? dimensions,
        IReadOnlyCollection<string>? images,
        ProductVariationStatus status,
        CancellationToken ct = default);

    Task DeleteVariationAsync(Guid variationId, CancellationToken ct = default);

    Task ReorderVariationsAsync(Guid productId, IReadOnlyList<Guid> orderedVariationIds, CancellationToken ct = default);

    Task SetDefaultVariationAsync(Guid productId, Guid variationId, CancellationToken ct = default);

    Task AssignCategoryAsync(Guid productId, Guid categoryId, CancellationToken ct = default);

    Task AssignTagAsync(Guid productId, Guid tagId, CancellationToken ct = default);

    Task RemoveCategoryAsync(Guid productId, Guid categoryId, CancellationToken ct = default);

    Task RemoveTagAsync(Guid productId, Guid tagId, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
