using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Contract.Events.Product;

using Product.Application.Abstractions.Persistence.Products;
using Product.Domain.ValueObjects;

namespace Product.Application.Features.Products.Commands.AddVariation;

public sealed class AddVariationHandler(
    IProductReadService productReadService,
    IProductWriteService productWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<AddVariationCommand, AddVariationResponse>
{
    public async Task<AddVariationResponse> Handle(AddVariationCommand request, CancellationToken ct = default)
    {
        // Check if exists product SKU
        if (await productReadService.SkuExistsAsync(request.Sku, ct: ct))
        {
            var owningProductName = await productReadService.GetProductNameBySkuAsync(request.Sku, ct);
            var ownerSuffix = owningProductName is null
                ? string.Empty
                : $" (used by product \"{owningProductName}\")";
            throw new ConflictException(
                systemMessage: $"Variation with SKU ({request.Sku}) already exists{ownerSuffix}",
                detail: new { owningProductName });
        }

        // Get product information
        var product = await productReadService.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException(nameof(ProductEntity), request.ProductId);

        // Initialize value objects
        var sku = Sku.Create(request.Sku);
        var barcode = request.Barcode is null
            ? null
            : Barcode.Create(request.Barcode);
        // Create new dimensions if providing
        var dimensions = request.DimensionsLength is not null
            && request.DimensionsWidth is not null
            && request.DimensionsHeight is not null
                ? Dimensions.Create(
                    request.DimensionsLength.Value,
                    request.DimensionsWidth.Value,
                    request.DimensionsHeight.Value)
                : null;

        ProductVariation variation = null!;
        var correlationId = currentUser.GetCorrelationId() ?? Guid.NewGuid().ToString();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            // Create product variation from DB
            variation = await productWriteService.AddVariationAsync(
                request.ProductId,
                sku,
                request.Price,
                barcode,
                request.Cost,
                request.Weight,
                dimensions,
                request.Images,
                makeDefault: request.MakeDefault,
                ct);

            // Publish variation created event bus
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
        }, ct: ct);

        return new AddVariationResponse(ProductVariationResponse.From(variation));
    }
}
