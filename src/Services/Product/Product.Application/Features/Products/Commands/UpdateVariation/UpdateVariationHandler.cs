using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Contract.Events.Product;

using Product.Application.Abstractions.Persistence.Products;
using Product.Domain.ValueObjects;

namespace Product.Application.Features.Products.Commands.UpdateVariation;

public sealed class UpdateVariationHandler(
    IProductReadService productReadService,
    IProductWriteService productWriteService,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<UpdateVariationCommand, UpdateVariationResponse>
{
    public async Task<UpdateVariationResponse> Handle(UpdateVariationCommand request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<ProductVariationStatus>(request.Status, ignoreCase: true, out var status))
            throw new BadRequestException($"Invalid variation status: {request.Status}");

        if (await productReadService.SkuExistsAsync(request.Sku, request.VariationId, ct))
            throw new ConflictException($"Variation with SKU ({request.Sku}) already exists");

        var dimensions = request.DimensionsLength is not null && request.DimensionsWidth is not null && request.DimensionsHeight is not null
            ? Dimensions.Create(request.DimensionsLength.Value, request.DimensionsWidth.Value, request.DimensionsHeight.Value)
            : null;
        var barcode = request.Barcode is null ? null : Barcode.Create(request.Barcode);
        var sku = Sku.Create(request.Sku);
        var correlationId = currentUser.GetCorrelationId() ?? Guid.NewGuid().ToString();

        await outboxStore.EnqueueAsync(
            new ProductVariationUpdatedIntegrationEvent(
                request.ProductId, request.VariationId, request.Sku, request.Price, status.ToString(), correlationId),
            ct);

        await productWriteService.UpdateAsync(request.ProductId, async (product) =>
        {
            var variation = product.Variations.FirstOrDefault(v => v.Id == request.VariationId)
                ?? throw new NotFoundException(nameof(ProductVariation), request.VariationId);

            variation.UpdateIdentifiers(sku, barcode);
            variation.UpdatePricing(request.Price, request.Cost);
            variation.UpdatePhysicalAttributes(request.Weight, dimensions);
            variation.ReplaceImages(request.Images ?? []);

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

        return new UpdateVariationResponse();
    }
}
