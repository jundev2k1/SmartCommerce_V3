using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Contract.Events.Product;

using Product.Application.Abstractions.Persistence.Products;

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
        var variationInput = request.VariationInput;

        // Check if exists product SKU
        if (await productReadService.SkuExistsAsync(variationInput.Sku, ct: ct))
        {
            var owningProductName = await productReadService.GetProductNameBySkuAsync(variationInput.Sku, ct);
            var ownerSuffix = owningProductName is null
                ? string.Empty
                : $" (used by product \"{owningProductName}\")";
            throw new ConflictException(
                systemMessage: $"Variation with SKU ({variationInput.Sku}) already exists{ownerSuffix}",
                detail: new { owningProductName });
        }

        // Get and check if product exists by ID
        var targetProduct = await productReadService.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException(nameof(request.ProductId), request.ProductId);

        var correlationId = currentUser.GetCorrelationId() ?? Guid.NewGuid().ToString();

        ProductVariation variation = null!;
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            // Create product variation from DB
            variation = await productWriteService.AddVariationAsync(
                request.ProductId,
                variationInput,
                ct);

            // Publish variation created event bus
            await outboxStore.EnqueueAsync(
                new ProductVariationCreatedIntegrationEvent(
                    variation.ProductId,
                    variation.Id,
                    variation.Sku.Value,
                    targetProduct.Name,
                    variation.Name,
                    variation.Price,
                    variation.Status.ToString(),
                    correlationId),
                ct);
        }, ct: ct);

        return new AddVariationResponse(ProductVariationResponse.From(variation));
    }
}
