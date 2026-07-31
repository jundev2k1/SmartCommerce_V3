using Product.Application.Features.Products.DTOs;

namespace Product.Application.Abstractions.Persistence.Products;

public sealed record CreateProductRequest(
    string Code,
    string Name,
    string Description,
    string Slug,
    IReadOnlyCollection<ProductVariationInputDto> Variations,
    IReadOnlyCollection<Guid> CategoryIds,
    IReadOnlyCollection<Guid> TagIds);

public interface IProductWriteService
{
    /// <summary>Returns the created ProductEntity - CreateProductHandler needs it whole (Id, DefaultVariation, every variation) to build ProductCreatedIntegrationEvent/ProductVariationCreatedIntegrationEvent per variation.</summary>
    Task<ProductEntity> CreateAsync(CreateProductRequest request, CancellationToken ct = default);

    Task UpdateDetailsAsync(
        Guid id,
        string name,
        string description,
        Slug slug,
        CancellationToken ct = default);

    Task<ProductVariation> AddVariationAsync(
        Guid productId,
        ProductVariationInputDto variation,
        CancellationToken ct = default);

    Task<ProductVariation[]> AddVariationsAsync(
        Guid productId,
        IEnumerable<ProductVariationInputDto> variations,
        CancellationToken ct = default);

    Task<ProductVariation> UpdateVariationInformationAsync(
        Guid productId,
        Guid variationId,
        Sku sku,
        string name,
        Money price,
        Barcode? barcode,
        Weight? weight,
        Dimensions? dimensions,
        IReadOnlyCollection<string>? images,
        ProductVariationStatus? status = null,
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
