using BuildingBlock.Application.Exceptions;
using BuildingBlock.Persistence.Repository;

using Product.Application.Abstractions.Persistence.Products;
using Product.Domain.Enums;
using Product.Domain.ValueObjects;

namespace Product.Persistence.Contexts.Products.Write;

/// <summary>
/// Every method here only stages repository operations - it never calls IUnitOfWork itself.
/// The Application handler owns the transaction (ExecuteTransactionAsync) and its own
/// SaveChangesAsync, since every Product mutation historically committed that way; EF Core
/// automatically enlists this service's repo calls in whatever transaction the caller opened.
/// </summary>
public sealed class ProductWriteService(
    IRepository<ProductEntity> repo) : IProductWriteService
{
    public async Task CreateAsync(ProductEntity product, CancellationToken ct = default)
    {
        await repo.AddAsync(product, ct);
    }

    public async Task UpdateDetailsAsync(Guid id, string name, string description, string slug, CancellationToken ct = default)
    {
        await repo.UpdateAsync(id, async product =>
        {
            product.UpdateDetails(name, description);
            product.ChangeSlug(Slug.Create(slug));
        }, ct);
    }

    public async Task<ProductVariation> AddVariationAsync(
        Guid productId,
        Sku sku,
        decimal price,
        Barcode? barcode,
        decimal? cost,
        decimal? weight,
        Dimensions? dimensions,
        IEnumerable<string>? images,
        bool makeDefault,
        CancellationToken ct = default)
    {
        ProductVariation variation = null!;

        await repo.UpdateAsync(productId, async product =>
        {
            variation = product.AddVariation(sku, price, barcode, cost, weight, dimensions, images, makeDefault: makeDefault);
            await Task.CompletedTask;
        }, ct);

        return variation;
    }

    public async Task UpdateVariationAsync(
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
        CancellationToken ct = default)
    {
        await repo.UpdateAsync(productId, async product =>
        {
            var variation = product.Variations.FirstOrDefault(v => v.Id == variationId)
                ?? throw new NotFoundException(nameof(ProductVariation), variationId);

            variation.UpdateIdentifiers(sku, barcode);
            variation.UpdatePricing(price, cost);
            variation.UpdatePhysicalAttributes(weight, dimensions);
            variation.ReplaceImages(images ?? []);

            switch (status)
            {
                case ProductVariationStatus.Active:
                    variation.Activate();
                    break;
                case ProductVariationStatus.Inactive:
                    variation.Deactivate();
                    break;
                case ProductVariationStatus.Discontinued:
                    variation.Discontinue();
                    break;
            }

            await Task.CompletedTask;
        }, ct);
    }

    public async Task DeleteVariationAsync(Guid productId, Guid variationId, CancellationToken ct = default)
    {
        await repo.UpdateAsync(productId, async product =>
        {
            product.RemoveVariation(variationId);
            await Task.CompletedTask;
        }, ct);
    }

    public async Task ReorderVariationsAsync(Guid productId, IReadOnlyList<Guid> orderedVariationIds, CancellationToken ct = default)
    {
        await repo.UpdateAsync(productId, async product =>
        {
            var currentIds = product.Variations.Select(v => v.Id).ToHashSet();
            var requestedIds = orderedVariationIds.ToHashSet();

            if (requestedIds.Count != orderedVariationIds.Count || !currentIds.SetEquals(requestedIds))
            {
                throw new BadRequestException(
                    "OrderedVariationIds must contain exactly every existing variation id, each exactly once.");
            }

            for (var i = 0; i < orderedVariationIds.Count; i++)
            {
                var variation = product.Variations.First(v => v.Id == orderedVariationIds[i]);
                variation.ChangeDisplayOrder(i);
            }

            await Task.CompletedTask;
        }, ct);
    }

    public async Task SetDefaultVariationAsync(Guid productId, Guid variationId, CancellationToken ct = default)
    {
        await repo.UpdateAsync(productId, async product =>
        {
            product.SetDefaultVariation(variationId);
            await Task.CompletedTask;
        }, ct);
    }

    public async Task AssignCategoryAsync(Guid productId, Guid categoryId, CancellationToken ct = default)
    {
        await repo.UpdateAsync(productId, async product =>
        {
            product.AssignCategory(categoryId);
            await Task.CompletedTask;
        }, ct);
    }

    public async Task AssignTagAsync(Guid productId, Guid tagId, CancellationToken ct = default)
    {
        await repo.UpdateAsync(productId, async product =>
        {
            product.AssignTag(tagId);
            await Task.CompletedTask;
        }, ct);
    }

    public async Task RemoveCategoryAsync(Guid productId, Guid categoryId, CancellationToken ct = default)
    {
        await repo.UpdateAsync(productId, async product =>
        {
            product.RemoveCategory(categoryId);
            await Task.CompletedTask;
        }, ct);
    }

    public async Task RemoveTagAsync(Guid productId, Guid tagId, CancellationToken ct = default)
    {
        await repo.UpdateAsync(productId, async product =>
        {
            product.RemoveTag(tagId);
            await Task.CompletedTask;
        }, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await repo.DeleteAsync(id, ct);
    }
}
