using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Contract.Events.Product;

using Product.Application.Abstractions.Repositories;
using Product.Domain.ValueObjects;

namespace Product.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductHandler(
    IProductRepository productRepo,
    IProductCategoryRepository categoryRepo,
    IProductTagRepository tagRepo,
    IUnitOfWork uow,
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

        if (await productRepo.CodeExistsAsync(request.Code, ct))
            throw new ConflictException($"Product with code ({request.Code}) already exists");

        if (await productRepo.SlugExistsAsync(request.Slug, ct: ct))
            throw new ConflictException($"Product with slug ({request.Slug}) already exists");

        foreach (var variation in request.Variations)
        {
            if (await productRepo.SkuExistsAsync(variation.Sku, ct: ct))
                throw new ConflictException($"Variation with SKU ({variation.Sku}) already exists");
        }
    }

    private async Task ValidateCategoriesAsync(IReadOnlyCollection<Guid> categoryIds, CancellationToken ct)
    {
        foreach (var categoryId in categoryIds)
        {
            _ = await categoryRepo.GetByIdAsync(categoryId, ct)
                ?? throw new NotFoundException(nameof(ProductCategory), categoryId);
        }
    }

    private async Task ValidateTagsAsync(IReadOnlyCollection<Guid> tagIds, CancellationToken ct)
    {
        foreach (var tagId in tagIds)
        {
            _ = await tagRepo.GetByIdAsync(tagId, ct)
                ?? throw new NotFoundException(nameof(ProductTag), tagId);
        }
    }
    #endregion

    #region Product Creation
    private static ProductEntity CreateNewProduct(
        CreateProductCommand request, IReadOnlyCollection<Guid> categoryIds, IReadOnlyCollection<Guid> tagIds)
    {
        var variationModels = request.Variations.Select(ToCreateModel);

        var product = ProductEntity.Create(
            Guid.CreateVersion7(),
            ProductCode.Create(request.Code),
            request.Name.Trim(),
            request.Description?.Trim() ?? string.Empty,
            Slug.Create(request.Slug),
            variationModels);

        foreach (var categoryId in categoryIds)
            product.AssignCategory(categoryId);

        foreach (var tagId in tagIds)
            product.AssignTag(tagId);

        return product;
    }

    private static ProductVariationCreateModel ToCreateModel(CreateProductVariationInput input)
    {
        return new ProductVariationCreateModel(
            Sku.Create(input.Sku),
            input.Price,
            input.IsDefault,
            input.Barcode is null ? null : Barcode.Create(input.Barcode),
            input.Cost,
            input.Weight,
            input.DimensionsLength is not null && input.DimensionsWidth is not null && input.DimensionsHeight is not null
                ? Dimensions.Create(input.DimensionsLength.Value, input.DimensionsWidth.Value, input.DimensionsHeight.Value)
                : null,
            input.Images);
    }
    #endregion

    #region Persistence
    private async Task SaveProductAsync(ProductEntity product, string correlationId, CancellationToken ct)
    {
        await uow.ExecuteTransactionAsync(async () =>
        {
            await productRepo.AddAsync(product, ct);
            await PublishIntegrationEventsAsync(product, correlationId, ct);
        }, ct: ct);
    }

    private async Task PublishIntegrationEventsAsync(ProductEntity product, string correlationId, CancellationToken ct)
    {
        await outboxStore.EnqueueAsync(
            new ProductCreatedIntegrationEvent(product.Id, product.Code.Value, product.Name, product.Slug.Value, correlationId),
            ct);

        foreach (var variation in product.Variations)
        {
            await outboxStore.EnqueueAsync(
                new ProductVariationCreatedIntegrationEvent(
                    product.Id, variation.Id, variation.Sku.Value, product.Name, variation.Price, variation.Status.ToString(), correlationId),
                ct);
        }
    }
    #endregion
}
