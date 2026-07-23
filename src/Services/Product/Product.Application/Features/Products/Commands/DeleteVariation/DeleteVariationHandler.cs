using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Contract.Events.Product;

using Product.Application.Abstractions.Persistence.Products;

namespace Product.Application.Features.Products.Commands.DeleteVariation;

/// <summary>Aggregate enforces "cannot remove the last variation" and auto-promotes a new Default if needed - see Product.RemoveVariation.</summary>
public sealed class DeleteVariationHandler(
    IProductWriteService productWriteService,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<DeleteVariationCommand, DeleteVariationResponse>
{
    public async Task<DeleteVariationResponse> Handle(DeleteVariationCommand request, CancellationToken ct = default)
    {
        var correlationId = currentUser.GetCorrelationId() ?? Guid.NewGuid().ToString();

        await outboxStore.EnqueueAsync(
            new ProductVariationDeletedIntegrationEvent(request.ProductId, request.VariationId, correlationId),
            ct);

        await productWriteService.UpdateAsync(request.ProductId, async (product) =>
        {
            product.RemoveVariation(request.VariationId);
            await Task.CompletedTask;
        }, ct);

        return new DeleteVariationResponse();
    }
}
