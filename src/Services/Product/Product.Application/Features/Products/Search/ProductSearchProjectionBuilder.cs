using Product.Application.Abstractions.Persistence.ProductCategories;
using Product.Application.Abstractions.Persistence.ProductTags;
using Product.Application.Abstractions.Search;

namespace Product.Application.Features.Products.Search;

/// <summary>
/// Integration Event -&gt; Search Document. The only place ProductSearchDocument is assembled -
/// reused by both the live sync path (OnProductSearchSyncRequiredHandler) and the rebuild path
/// (RebuildProductSearchIndexHandler), so future schema changes only touch this one class. See
/// docs/reference/search.md.
/// </summary>
public sealed class ProductSearchProjectionBuilder(
    IProductCategoryReadService categoryReadService,
    IProductTagReadService tagReadService)
{
    public async Task<ProductSearchDocument> BuildAsync(ProductEntity product, CancellationToken ct = default)
    {
        var categories = await categoryReadService.GetAllAsync(ct);
        var tags = await tagReadService.GetAllAsync(ct);
        return Build(product, categories, tags);
    }

    public async Task<IReadOnlyList<ProductSearchDocument>> BuildManyAsync(
        IReadOnlyList<ProductEntity> products, CancellationToken ct = default)
    {
        var categories = await categoryReadService.GetAllAsync(ct);
        var tags = await tagReadService.GetAllAsync(ct);
        return [.. products.Select(p => Build(p, categories, tags))];
    }

    private static ProductSearchDocument Build(
        ProductEntity product, IReadOnlyList<ProductCategory> categories, IReadOnlyList<ProductTag> tags)
    {
        var defaultVariation = product.DefaultVariation;
        var categoryIds = product.CategoryMappings.Select(m => m.CategoryId).ToList();
        var tagIds = product.TagMappings.Select(m => m.TagId).ToList();

        return new ProductSearchDocument
        {
            ProductId = product.Id,
            Code = product.Code.Value,
            Name = product.Name,
            Slug = product.Slug.Value,
            Thumbnail = defaultVariation.Images.FirstOrDefault(),
            DefaultPrice = defaultVariation.Price,
            DefaultVariationId = defaultVariation.Id,
            DefaultVariationSku = defaultVariation.Sku.Value,
            CategoryIds = categoryIds,
            CategoryNames = [.. categories.Where(c => categoryIds.Contains(c.Id)).Select(c => c.Name)],
            TagIds = tagIds,
            TagNames = [.. tags.Where(t => tagIds.Contains(t.Id)).Select(t => t.Name)],
            // Product itself has no lifecycle status field today - the Default variation's
            // status is the documented stand-in (see docs/reference/search.md).
            Status = defaultVariation.Status.ToString(),
            UpdatedAt = DateTime.UtcNow,
        };
    }
}
