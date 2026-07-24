using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Contract.Events.Product;
using BuildingBlock.SharedKernel.Extensions;

using Product.Application.Abstractions.Persistence.ProductCategories;
using Product.Application.Abstractions.Persistence.Products;
using Product.Application.Abstractions.Persistence.ProductTags;
using Product.Application.Features.Products.Mapping;

namespace Product.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductHandler(
    IProductReadService productReadService,
    IProductWriteService productWriteService,
    IProductCategoryReadService categoryReadService,
    IProductTagReadService tagReadService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken ct = default)
    {
        // Validate request and referenced resources.
        await ValidateRequestAsync(request, ct);

        var categoryIds = request.CategoryIds ?? [];
        await ValidateCategoriesAsync(categoryIds, ct);

        var tagIds = request.TagIds ?? [];
        await ValidateTagsAsync(tagIds, ct);

        // Create the aggregate.
        var product = CreateNewProduct(request, categoryIds, tagIds);

        // Persist data and enqueue integration events.
        var correlationId = currentUser.GetCorrelationId() ?? Guid.NewGuid().ToString();
        await SaveProductAsync(product, correlationId, ct);

        // Build response.
        return new CreateProductResponse(product.Id, product.DefaultVariation.Id);
    }

    #region Validation
    private async Task ValidateRequestAsync(CreateProductCommand request, CancellationToken ct)
    {
        if (request.Variations.Count == 0)
            throw new BadRequestException("A product must be created with at least one variation.");

        if (await productReadService.CodeExistsAsync(request.Code, ct))
            throw new ConflictException($"Product with code ({request.Code}) already exists");

        if (await productReadService.SlugExistsAsync(request.Slug, ct: ct))
            throw new ConflictException($"Product with slug ({request.Slug}) already exists");

        foreach (var variation in request.Variations)
        {
            if (await productReadService.SkuExistsAsync(variation.Sku, ct: ct))
                throw new ConflictException($"Variation with SKU ({variation.Sku}) already exists");
        }
    }

    private async Task ValidateCategoriesAsync(IReadOnlyCollection<Guid> categoryIds, CancellationToken ct)
    {
        var existingCategoryIds = await categoryReadService.GetExistingIdsAsync(categoryIds, ct);
        var nonExistIds = categoryIds.Except(existingCategoryIds)
            .Select(t => t.ToString())
            .ToArray();
        if (nonExistIds.Length > 0)
            throw new NotFoundException(nameof(ProductCategories), nonExistIds.JoinToString(","));
    }

    private async Task ValidateTagsAsync(IReadOnlyCollection<Guid> tagIds, CancellationToken ct)
    {
        var existingTagIds = await tagReadService.GetExistingTagIdsAsync(tagIds, ct);
        var nonExistIds = tagIds.Except(existingTagIds)
            .Select(t => t.ToString())
            .ToArray();
        if (nonExistIds.Length > 0)
            throw new NotFoundException(nameof(ProductTag), nonExistIds.JoinToString(","));
    }
    #endregion

    #region Product Creation
    private static ProductEntity CreateNewProduct(
        CreateProductCommand request,
        IReadOnlyCollection<Guid> categoryIds,
        IReadOnlyCollection<Guid> tagIds)
    {
        var variations = request.Variations.MapInputToEntities(default);
        var product = ProductEntity.Create(
            ProductCode.Create(request.Code),
            request.Name,
            request.Description,
            Slug.Create(request.Slug),
            null,
            variations,
            categoryIds,
            tagIds);

        return product;
    }
    #endregion

    #region Persistence
    private async Task SaveProductAsync(
        ProductEntity product,
        string correlationId,
        CancellationToken ct)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productWriteService.CreateAsync(product, ct);
            await PublishIntegrationEventsAsync(product, correlationId, ct);
        }, ct: ct);
    }

    private async Task PublishIntegrationEventsAsync(
        ProductEntity product,
        string correlationId,
        CancellationToken ct)
    {
        await outboxStore.EnqueueAsync(
            new ProductCreatedIntegrationEvent(
                product.Id,
                product.Code.Value,
                product.Name,
                product.Slug.Value,
                correlationId),
            ct);

        foreach (var variation in product.Variations)
        {
            await outboxStore.EnqueueAsync(
                new ProductVariationCreatedIntegrationEvent(
                    product.Id,
                    variation.Id,
                    variation.Sku.Value,
                    product.Name,
                    variation.Price,
                    variation.Status.ToString(),
                    correlationId),
                ct);
        }
    }
    #endregion
}
