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
        if (await productReadService.SkuExistsAsync(request.Sku, ct: ct))
        {
            // SKU uniqueness is global, not scoped per-product (intentional - see
            // docs/tasks/2026-07-22/Task1_verify-add-variation-contract.md) - naming which
            // product currently owns it saves a round-trip of confused support/QA questions.
            var owningProductName = await productReadService.GetProductNameBySkuAsync(request.Sku, ct);
            var ownerSuffix = owningProductName is null ? string.Empty : $" (used by product \"{owningProductName}\")";
            throw new ConflictException($"Variation with SKU ({request.Sku}) already exists{ownerSuffix}");
        }

        var product = await productReadService.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException(nameof(ProductEntity), request.ProductId);

        var sku = Sku.Create(request.Sku);
        var barcode = request.Barcode is null ? null : Barcode.Create(request.Barcode);
        var dimensions = request.DimensionsLength is not null && request.DimensionsWidth is not null && request.DimensionsHeight is not null
            ? Dimensions.Create(request.DimensionsLength.Value, request.DimensionsWidth.Value, request.DimensionsHeight.Value)
            : null;

        ProductVariation variation = null!;
        var correlationId = currentUser.GetCorrelationId() ?? Guid.NewGuid().ToString();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            variation = await productWriteService.AddVariationAsync(
                request.ProductId, sku, request.Price, barcode, request.Cost, request.Weight, dimensions, request.Images,
                makeDefault: request.MakeDefault, ct);

            await outboxStore.EnqueueAsync(
                new ProductVariationCreatedIntegrationEvent(
                    product.Id, variation.Id, variation.Sku.Value, product.Name, variation.Price, variation.Status.ToString(), correlationId),
                ct);
        }, ct: ct);

        return new AddVariationResponse(ProductVariationResponse.From(variation));
    }
}
